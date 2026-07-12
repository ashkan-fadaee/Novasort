using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NovaSortPro.Models;
using NovaSortPro.Services;

namespace NovaSortPro.ViewModels
{
    public class RulesViewModel : ObservableObject
    {
        private readonly RuleService _ruleService;
        private readonly LoggingService _logger;

        private ObservableCollection<RuleItem> _rules = new();
        private string _newPattern = string.Empty;
        private string _newTargetFolder = string.Empty;

        public RulesViewModel(RuleService ruleService, LoggingService logger)
        {
            _ruleService = ruleService;
            _logger = logger;

            AddRuleCommand = new RelayCommand(ExecuteAddRule);
            DeleteRuleCommand = new RelayCommand<RuleItem>(ExecuteDeleteRule);
            SaveRulesCommand = new RelayCommand(ExecuteSaveRules);
            ExportRulesCommand = new RelayCommand<string>(ExecuteExportRules);
            ImportRulesCommand = new RelayCommand<string>(ExecuteImportRules);

            LoadRules();
        }

        public ObservableCollection<RuleItem> Rules
        {
            get => _rules;
            set => SetProperty(ref _rules, value);
        }

        public string NewPattern
        {
            get => _newPattern;
            set => SetProperty(ref _newPattern, value);
        }

        public string NewTargetFolder
        {
            get => _newTargetFolder;
            set => SetProperty(ref _newTargetFolder, value);
        }

        public ICommand AddRuleCommand { get; }
        public ICommand DeleteRuleCommand { get; }
        public ICommand SaveRulesCommand { get; }
        public ICommand ExportRulesCommand { get; }
        public ICommand ImportRulesCommand { get; }

        public void LoadRules()
        {
            var ruleList = _ruleService.GetRules();
            Rules = new ObservableCollection<RuleItem>(ruleList);
        }

        private void ExecuteAddRule()
        {
            if (string.IsNullOrWhiteSpace(NewPattern) || string.IsNullOrWhiteSpace(NewTargetFolder))
                return;

            var newRule = new RuleItem
            {
                Pattern = NewPattern.Trim().ToLowerInvariant(),
                TargetFolder = NewTargetFolder.Trim(),
                IsActive = true,
                IsCustom = true
            };

            try
            {
                _ruleService.AddRule(newRule);
                Rules.Add(newRule);
                NewPattern = string.Empty;
                NewTargetFolder = string.Empty;
                _logger.Log($"Added custom rule: {newRule.Pattern} -> {newRule.TargetFolder}");
            }
            catch (Exception ex)
            {
                _logger.Log($"Failed to add rule: {ex.Message}", "ERROR");
            }
        }

        private void ExecuteDeleteRule(RuleItem? rule)
        {
            if (rule == null) return;

            try
            {
                _ruleService.DeleteRule(rule.Id);
                Rules.Remove(rule);
                _logger.Log($"Deleted rule: {rule.Pattern}");
            }
            catch (Exception ex)
            {
                _logger.Log($"Failed to delete rule: {ex.Message}", "ERROR");
            }
        }

        private void ExecuteSaveRules()
        {
            try
            {
                foreach (var rule in Rules)
                {
                    _ruleService.UpdateRule(rule);
                }
                _logger.Log("All rules updated and saved to local DB");
            }
            catch (Exception ex)
            {
                _logger.Log($"Failed to save rules: {ex.Message}", "ERROR");
            }
        }

        private void ExecuteExportRules(string? targetPath)
        {
            if (string.IsNullOrEmpty(targetPath)) return;

            try
            {
                var json = _ruleService.ExportRules();
                File.WriteAllText(targetPath, json);
                _logger.Log($"Rules exported successfully to {targetPath}");
            }
            catch (Exception ex)
            {
                _logger.Log($"Failed to export rules: {ex.Message}", "ERROR");
            }
        }

        private void ExecuteImportRules(string? targetPath)
        {
            if (string.IsNullOrEmpty(targetPath) || !File.Exists(targetPath)) return;

            try
            {
                var json = File.ReadAllText(targetPath);
                _ruleService.ImportRules(json);
                LoadRules();
                _logger.Log($"Rules imported successfully from {targetPath}");
            }
            catch (Exception ex)
            {
                _logger.Log($"Failed to import rules: {ex.Message}", "ERROR");
            }
        }
    }
}
