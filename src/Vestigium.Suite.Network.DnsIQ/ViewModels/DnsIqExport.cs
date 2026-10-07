    public void RememberExportChecks(bool lookup, bool capture, bool probe, bool openAfter, bool openFolder)
    {
        _exportQuiet = true;
        ExportLookup = lookup;
        ExportCapture = capture;
        ExportProbe = probe;
        ExportOpenAfter = openAfter;
        ExportOpenFolder = openFolder;
        _exportQuiet = false;
        ExportSelectedCommand.NotifyCanExecuteChanged();
        Session?.Save();
    }
