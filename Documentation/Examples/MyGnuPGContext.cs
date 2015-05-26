class MyGnuPGContext : GnuPGContext
{
    public MyGnuPGContext () : base ()
    {
    }

    protected override string GetPasswordForKey (PgpSecretKey key)
    {
        // prompt the user (or a secure password cache) for the password for the specified secret key.
        return "password";
    }
}
