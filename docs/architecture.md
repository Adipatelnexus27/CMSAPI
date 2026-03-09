# CMSAPI Architecture Notes

- Clean Architecture boundaries are mandatory.
- Controllers depend on Application contracts only.
- Infrastructure implements repository/service interfaces.
- SQL access will be implemented through ADO.NET and stored procedures.
