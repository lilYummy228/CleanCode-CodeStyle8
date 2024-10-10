using System;
using System.Data;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;

namespace CleanCode_CodeStyle8
{
    public class View : IView
    {
        private Presenter _presenter;

        public string PassportTextbox { get; private set; }
        public string TextResult { get => TextResult; set => Show(value); }

        public void Show(string message) =>
            Console.WriteLine(message);

        public void SetPassportData()
        {
            string value = Console.ReadLine();

            if (value == string.Empty)
                throw new ArgumentNullException(nameof(value));

            PassportTextbox = value;
        }

        public void SetTextResult() => _presenter.GetAccessState(PassportTextbox);

        public void SetError() =>
            TextResult = "Неверный формат серии или номера паспорта";
    }

    public interface IView
    {
        string PassportTextbox { get; }
        string TextResult { get; }

        void Show(string message);

        void SetPassportData();

        void SetError();
    }

    public class Presenter
    {
        private IView _view;
        private PassportQuery _passportQuery;
        private DataBaseContext _dataBaseContext;
        private int _dataLength = 10;
        private int _exceptionNumber = 1;
        private string _rawData;

        public void GetAccessState(string textbox)
        {
            CheckData();

            if (_passportQuery.Data == null)
                _view.Show("Паспорт «" + textbox + "» в списке участников дистанционного голосования НЕ НАЙДЕН");

            if (_dataBaseContext.IsExist)
                _view.Show("По паспорту «" + textbox + "» доступ к бюллетеню на дистанционном электронном голосовании ПРЕДОСТАВЛЕН");
            else
                _view.Show("По паспорту «" + textbox + "» доступ к бюллетеню на дистанционном электронном голосовании НЕ ПРЕДОСТАВЛЯЛСЯ");
        }

        public void CheckData()
        {
            if (_view.PassportTextbox.Trim() == "")
            {
                _view.Show("Введите серию и номер паспорта");
                _view.SetPassportData();
            }
            else
            {
                _rawData = _view.PassportTextbox.Trim().Replace(" ", string.Empty);

                if (_rawData.Length < _dataLength)
                    _view.SetError();
                else
                    _dataBaseContext.SetDataTable(Hasher.ComputeSha256Hash(_rawData));
            }
        }

        public void ShowExceptionReport(SQLiteException exception)
        {
            if (exception.ErrorCode != _exceptionNumber)
                return;

            _view.Show("Файл db.sqlite не найден. Положите файл в папку вместе с exe.");
        }
    }

    public class DataBaseContext
    {
        private Presenter _presenter;
        private PassportQuery _passportQuery;

        public bool IsExist { get; private set; }

        public void SetDataTable(string passportHash)
        {
            string commandText = string.Format("select * from passports where num='{0}' limit 1;", passportHash);
            string connectionString = string.Format("Data Source=" + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\db.sqlite");

            try
            {
                SQLiteConnection connection = new SQLiteConnection(connectionString);

                connection.Open();

                SQLiteDataAdapter sqLiteDataAdapter = new SQLiteDataAdapter(new SQLiteCommand(commandText, connection));

                DataTable dataTable = new DataTable();

                sqLiteDataAdapter.Fill(dataTable);

                IsExist = _passportQuery.IsExist(dataTable);
            }
            catch (SQLiteException exception)
            {
                _presenter.ShowExceptionReport(exception);
            }
        }
    }

    public class PassportQuery
    {
        public DataTable Data { get; private set; }

        public bool IsExist(DataTable dataTable)
        {
            if (dataTable.Rows.Count > 0)
            {
                if (dataTable.Rows[0].ItemArray[1] != null)
                {
                    Data = dataTable;

                    return true;
                }
            }

            return false;
        }
    }

    [Serializable]
    public class SQLiteException : Exception
    {
        public SQLiteException() { }

        public SQLiteException(string message) : base(message) { }

        public SQLiteException(string message, Exception innerException) : base(message, innerException) { }

        protected SQLiteException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public int ErrorCode { get; private set; }
    }

    public class SQLiteCommand
    {
        private string _commandText;
        private SQLiteConnection _connection;

        public SQLiteCommand(string commandText, SQLiteConnection connection)
        {
            _commandText = commandText;
            _connection = connection;
        }
    }

    public class SQLiteDataAdapter
    {
        private SQLiteCommand _sQLiteCommand;

        public SQLiteDataAdapter(SQLiteCommand sQLiteCommand) =>
            _sQLiteCommand = sQLiteCommand;

        public DataTable Fill(DataTable dataTable2) =>
            throw new NotImplementedException();
    }

    public class SQLiteConnection
    {
        private string _connectionString;

        public SQLiteConnection(string connectionString) =>
            _connectionString = connectionString;

        public void Open() =>
            throw new NotImplementedException();

        public void Close() =>
            throw new NotImplementedException();
    }

    public class Hasher
    {
        public static string ComputeSha256Hash(string rawData)
        {
            SHA256 sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));

            return Convert.ToBase64String(bytes);
        }
    }
}