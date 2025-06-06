﻿using Client.Models;
using Client.PdfDoucments;
using Client.Services;
using Client.Services.MessageService;
using Client.Stores;
using Client.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Client.ViewModels
{
    public partial class PrintDisciplinesPageViewModel : ViewModelBase
    {
        private readonly IMessageService _messageService;

        private List<DisciplinePrintInfo>? _disciplinesPrintInfos;
        private DisciplineStatusThresholds? _disciplineStatusThresholds;

        public List<CatalogTypeInfo> CatalogTypeInfos { get; init; }
        public List<short> EduYears { get; init; }
        public List<SemesterInfo> SemesterInfos { get; init; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanExecute))]
        [NotifyCanExecuteChangedFor(nameof(PrintDisciplinesCommand))]
        private CatalogTypeInfo? _selectedCatalogInfo;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanExecute))]
        [NotifyCanExecuteChangedFor(nameof(PrintDisciplinesCommand))]
        private short? _selectedEduYear;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanExecute))]
        [NotifyCanExecuteChangedFor(nameof(PrintDisciplinesCommand))]
        private SemesterInfo? _selectedSemesterInfo;

        [ObservableProperty]
        private bool _isNeedGrouping;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanExecute))]
        [NotifyCanExecuteChangedFor(nameof(PrintDisciplinesCommand))]
        private int? _sortOption;

        public bool CanExecute => SelectedSemesterInfo is not null &&
            SelectedCatalogInfo is not null &&
            SelectedEduYear is not null &&
            SortOption is not null;

        public IRelayCommand CloseCommand { get; init; }

        public PrintDisciplinesPageViewModel(ApiService apiService, UserStore userStore,
            IMessageService messageService, IRelayCommand closeCommand,
            IEnumerable<CatalogTypeInfo> catalogTypes, IEnumerable<short> eduYears) : base(apiService, userStore)
        {
            _messageService = messageService;
            CloseCommand = closeCommand;

            CatalogTypeInfos = catalogTypes.ToList();
            EduYears = eduYears.ToList();
            _selectedEduYear = EduYears.FirstOrDefault();

            SemesterInfos =
            [
                new SemesterInfo { SemesterId = 1, SemesterName = "Осінній семестр" },
                new SemesterInfo { SemesterId = 2, SemesterName = "Весняний семестр" }
            ];

            _sortOption = 0;
        }

        [RelayCommand(CanExecute = nameof(CanExecute))]
        private async Task PrintDisciplines()
        {
            if (_disciplinesPrintInfos is not null && _disciplinesPrintInfos.Count == 0) return;

            var path = _messageService.ShowSaveFileDialog("Виберіть місце збереження відомості", "Pdf file|*.pdf");

            if (path is null) return;

            await ExecuteWithWaiting(async () =>
            {
                if (_disciplinesPrintInfos is null)
                    await LoadPrintInfoAsync();

                if (HasErrorMessage) return;

                _disciplinesPrintInfos = [.. (SortOption == 0 ? _disciplinesPrintInfos.OrderBy(d => d.DisciplineCode) :
                    _disciplinesPrintInfos.OrderByDescending(d => d.StudentsCount).ThenBy(d => d.DisciplineCode))];

                Dictionary<byte, List<DisciplinePrintInfo>> groupedDisciplines = null!;

                if (IsNeedGrouping)
                    groupedDisciplines = _disciplinesPrintInfos
                        .GroupBy(d => d.EduLevel)
                        .ToDictionary(g => g.Key, g => g.ToList());

                var reportDocument = IsNeedGrouping ?
                new DisciplineReportDocument(groupedDisciplines, _userStore.WorkerInfo.Faculty.FacultyName,
                    SelectedCatalogInfo.CatalogName, SelectedEduYear.Value, SelectedSemesterInfo.SemesterId,
                    _disciplineStatusThresholds) :
                new DisciplineReportDocument(_disciplinesPrintInfos, _userStore.WorkerInfo.Faculty.FacultyName,
                    SelectedCatalogInfo.CatalogName, SelectedEduYear.Value, SelectedSemesterInfo.SemesterId,
                    _disciplineStatusThresholds);

                ErrorMessage = await PdfGenerator.GeneratePdf(reportDocument, path);

                if (HasErrorMessage) return;

                _messageService.ShowInfoMessage("Відомість успішно сформована", "Успіх");

                CloseCommand.Execute(null);
            });
        }

        private async Task LoadPrintInfoAsync()
        {
            (ErrorMessage, var response) =
                        await _apiService.GetAsync<JsonObject>("Discipline", $"getDisciplinesPrintInfo" +
                        $"?facultyId={_userStore.WorkerInfo.Faculty.FacultyId}" +
                        $"&catalogType={SelectedCatalogInfo.CatalogType}&eduYear={SelectedEduYear.Value}" +
                        $"&semester={SelectedSemesterInfo.SemesterId}", _userStore.AccessToken);

            if (HasErrorMessage) return;

            _disciplineStatusThresholds = JsonSerializer.Deserialize<DisciplineStatusThresholds>(response["thresholds"]);

            if (_disciplineStatusThresholds is null)
            {
                ErrorMessage = "Не вдалось отримати порогові значення для дисциплін";
                return;
            }

            _disciplinesPrintInfos = JsonSerializer.Deserialize<List<DisciplinePrintInfo>>(response["disciplines"]);

            if (_disciplinesPrintInfos is null)
            {
                _disciplinesPrintInfos = [];
                ErrorMessage = "Немає дисциплін, щоб формувати відомість";
            }
        }
    }
}
