using Microsoft.SqlServer.Server;
using System;
using System.Data;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;

namespace CleanCode_CodeStyle8
{
    public class Model
    {
        public PassportTextbox Textbox { get; private set; }
        public Form1 Form { get; private set; }

        public class PassportTextbox
        {
            public string Text { get => Text; set => GetData(); }
            public bool IsEmpty => Text.Trim() == "";
            public bool IsCorrect => Text.Length > 10;

            public int GetData()
            {
                string input = Console.ReadLine();

                if (input == string.Empty)
                    throw new ArgumentNullException(nameof(input));

                int data = Convert.ToInt32(input.Trim().Replace(" ", string.Empty));

                if (data <= 0)
                    throw new ArgumentOutOfRangeException(nameof(data));

                return data;
            }
        }

        public class Form1
        {
            public object ComputeSha256Hash(string rawData)
            {
                SHA256 sha256 = SHA256.Create();
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                return Convert.ToBase64String(bytes);
            }
        }        
    }

    public class Controller
    {
        private Model _model;
        private View _view;
        private string _textResult;

        public void ButtonClick()
        {
            if (_model.Textbox.IsEmpty)
            {
                _view.Message.Show(_view.Message.DataInput);
                _model.Textbox.GetData();
            }
            else
            {
                if (_model.Textbox.IsCorrect == false)
                {
                    _textResult = _view.Message.Incorrect;
                }
                else
                {
                    string commandText = string.Format(_view.Command.CommandText, _model.Form.ComputeSha256Hash(_model.Textbox.Text));
                    string connectionString = string.Format(_view.Command.CommandString);

                    try
                    {
                        SQLiteConnection connection = new SQLiteConnection(connectionString);

                        connection.Open();

                        SQLiteDataAdapter sqLiteDataAdapter = new SQLiteDataAdapter(new SQLiteCommand(commandText, connection));

                        DataTable dataTable1 = new DataTable();
                        DataTable dataTable2 = dataTable1;

                        sqLiteDataAdapter.Fill(dataTable2);

                        if (dataTable1.Rows.Count > 0)
                        {
                            if (Convert.ToBoolean(dataTable1.Rows[0].ItemArray[1]))
                                _textResult = "По паспорту «" + _model.Textbox.Text + "» доступ к бюллетеню на дистанционном электронном голосовании ПРЕДОСТАВЛЕН";
                            else
                                _textResult = "По паспорту «" + _model.Textbox.Text + "» доступ к бюллетеню на дистанционном электронном голосовании НЕ ПРЕДОСТАВЛЯЛСЯ";
                        }
                        else
                        {
                            _textResult = "Паспорт «" + _model.Textbox.Text + "» в списке участников дистанционного голосования НЕ НАЙДЕН";
                        }

                        connection.Close();
                    }
                    catch (SQLiteException ex)
                    {
                        if (ex.ErrorCode != 1)
                            return;

                        _view.Message.Show("Файл db.sqlite не найден. Положите файл в папку вместе с exe.");
                    }
                }
            }
        }

        public class SQLiteConnection
        {
            private string _connectionString;

            public SQLiteConnection(string connectionString)
            {
                if (connectionString == string.Empty)
                    throw new ArgumentNullException(nameof(connectionString));

                _connectionString = connectionString;
            }

            internal void Open()
            {
                throw new NotImplementedException();
            }

            internal void Close()
            {
                throw new NotImplementedException();
            }
        }

        public class SQLiteDataAdapter
        {
            private SQLiteCommand _command;

            public SQLiteDataAdapter(SQLiteCommand command)
            {
                if (command == null)
                    throw new ArgumentNullException();

                _command = command;
            }

            internal void Fill(DataTable dataTable2)
            {
                throw new NotImplementedException();
            }
        }

        public class SQLiteCommand
        {
            private string _commandText;
            private SQLiteConnection _connection;

            public SQLiteCommand(string commandText, SQLiteConnection connection)
            {
                if (commandText == string.Empty || connection == null)
                    throw new ArgumentNullException(nameof(commandText));

                _commandText = commandText;
                _connection = connection;
            }
        }

        [Serializable]
        internal class SQLiteException : Exception
        {
            public SQLiteException()
            {
            }

            public SQLiteException(string message) : base(message)
            {
            }

            public SQLiteException(string message, Exception innerException) : base(message, innerException)
            {
            }

            protected SQLiteException(SerializationInfo info, StreamingContext context) : base(info, context)
            {
            }

            public int ErrorCode { get; private set; }
        }
    }

    public class View
    {
        public MessageBox Message { get; private set; }
        public ConnectionMessage Command { get; private set; }

        public class MessageBox
        {
            public string Incorrect { get; private set; } = "Неверный формат серии или номера паспорта";
            public string DataInput { get; private set; } = "Введите серию и номер паспорта";

            public void Show(string message)
            {
                Console.WriteLine(message);
            }
        }

        public class ConnectionMessage
        {
            public string CommandText { get; private set; } = "select * from passports where num='{0}' limit 1;";
            public string CommandString { get; private set; } = $"Data Source= {Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\db.sqlite";
        }
    }
}