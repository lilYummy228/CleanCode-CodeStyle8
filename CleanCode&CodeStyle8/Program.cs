using System;
using System.Data;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;

namespace CleanCode_CodeStyle8
{
    internal class Program
    {
        private Passport _passport;
        private TextResult _textResult;

        public Program(Passport passport)
        {
            _passport = passport;
        }

        private void ButtonClick(object sender, EventArgs e)
        {
            if (_passport.Data.Trim() == "")
            {
                MessageBox.Request("Введите серию и номер паспорта");
                int passportData = Convert.ToInt32(Console.ReadLine());
            }
            else
            {
                string rawData = _passport.Data.Trim().Replace(" ", string.Empty);

                if (rawData.Length < _passport.DataCount)
                {
                    _textResult.Text = "Неверный формат серии или номера паспорта";
                }
                else
                {
                    string commandText = string.Format("select * from passports where num='{0}' limit 1;", Form1.ComputeSha256Hash(rawData));
                    string connectionString = string.Format("Data Source=" + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\db.sqlite");

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
                            {
                                _textResult.Text = "По паспорту «" + _passport.Data + "» доступ к бюллетеню на дистанционном электронном голосовании ПРЕДОСТАВЛЕН";
                            }
                            else
                            {
                                _textResult.Text = "По паспорту «" + _passport.Data + "» доступ к бюллетеню на дистанционном электронном голосовании НЕ ПРЕДОСТАВЛЯЛСЯ";
                            }
                        }
                        else
                        {
                            _textResult.Text = "Паспорт «" + _passport.Data + "» в списке участников дистанционного голосования НЕ НАЙДЕН";
                        }

                        connection.Close();
                    }
                    catch (SQLiteException exception)
                    {
                        if (exception.ErrorCode != exception.Error)
                            return;

                        MessageBox.Request("Файл db.sqlite не найден. Положите файл в папку вместе с exe.");
                        int num2 = Convert.ToInt32(Console.ReadLine());
                    }
                }
            }
        }

        private class TextResult
        {
            public string Text
            {
                set
                {
                    if (string.IsNullOrWhiteSpace(value))
                        throw new ArgumentNullException(nameof(value));

                    Text = value.ToString();
                }
            }
        }

        public class Form1
        {
            public static object ComputeSha256Hash(string rawData)
            {
                SHA256 sha256 = SHA256.Create();
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                return Convert.ToBase64String(bytes);
            }
        }

        public class Passport
        {
            public string Data { get; private set; }
            public int DataCount { get; private set; } = 10;
        }

        public class MessageBox
        {
            public static void Request(string request)
            {
                if (string.IsNullOrWhiteSpace(request))
                    throw new ArgumentNullException(nameof(request));

                Console.WriteLine(request);
            }
        }

        private class SQLiteConnection
        {
            string _connectionString;

            public SQLiteConnection(string connectionString)
            {
                _connectionString = connectionString != string.Empty ? connectionString : throw new ArgumentNullException(nameof(connectionString));
            }

            public void Open()
            {
                throw new NotImplementedException();
            }

            internal void Close()
            {
                throw new NotImplementedException();
            }
        }

        private class SQLiteDataAdapter
        {
            private SQLiteCommand _sQLiteCommand;

            public SQLiteDataAdapter(SQLiteCommand sQLiteCommand)
            {
                _sQLiteCommand = sQLiteCommand ?? throw new ArgumentNullException(nameof(sQLiteCommand));
            }

            internal void Fill(DataTable dataTable2)
            {
                throw new NotImplementedException();
            }
        }

        private class SQLiteCommand
        {
            string _commandText;
            SQLiteConnection _sQLiteConnection;

            public SQLiteCommand(string commandText, SQLiteConnection sQLiteConnection)
            {
                _commandText = commandText != string.Empty ? commandText : throw new ArgumentNullException(nameof(commandText));
                _sQLiteConnection = sQLiteConnection ?? throw new ArgumentNullException(nameof(sQLiteConnection));
            }
        }

        [Serializable]
        internal class SQLiteException : Exception
        {
            public SQLiteException() { }

            public SQLiteException(string message) : base(message) { }

            public SQLiteException(string message, Exception innerException) : base(message, innerException) { }

            protected SQLiteException(SerializationInfo info, StreamingContext context) : base(info, context) { }

            public int ErrorCode { get; private set; }
            public int Error { get; private set; } = 1;
        }
    }

}
