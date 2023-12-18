using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATodoList.ViewModels;

public class SettingsWindowViewModel : ViewModelBase
{
    private string _connectionString;

    private string _databaseName;

    public string ConnectionString
    {
        get => _connectionString;
        set => this.RaiseAndSetIfChanged(ref _connectionString, value);
    }

    public string DatabaseName
    {
        get => _databaseName;
        set => this.RaiseAndSetIfChanged(ref _databaseName, value);
    }

    public SettingsWindowViewModel()
    {
        if (Services.DatabaseService.DB is Services.MongoDBHelper db) {
            (_connectionString, _databaseName) = (db.ConnectionString, db.DatabaseName);
        } else {
            _connectionString = "127.0.0.1:27017";
            _databaseName = "ATodoList";
        }
    }

    public bool SwitchDatabase(string host, string db)
    {
        return Services.DatabaseService.TrySetMongoDBService(host, db);
    }
}
