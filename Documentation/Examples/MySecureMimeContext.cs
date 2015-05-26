
class MySecureMimeContext : DefaultSecureMimeContext
{
	public MySecureMimeContext ()
		: base (OpenDatabase ("C:\\wherever\\certdb.sqlite"))
	{
	}

	static IX509CertificateDatabase OpenDatabase (string fileName)
	{
		var builder = new SQLiteConnectionStringBuilder ();
		builder.DateTimeFormat = SQLiteDateFormats.Ticks;
		builder.DataSource = fileName;

		if (!File.Exists (fileName))
			SQLiteConnection.CreateFile (fileName);

		var sqlite = new SQLiteConnection (builder.ConnectionString);
		sqlite.Open ();

		return new SqliteCertificateDatabase (sqlite, "password");
	}
}
