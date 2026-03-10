-- Run with sqlcmd from CMSAPI/database/scripts:
-- sqlcmd -S "(LocalDb)\MSSQLLocalDB" -d "CMS" -i "010_audit_install.sql"

:r .\schema\011_audit_schema.sql
:r .\seeds\011_audit_seed.sql
:r .\stored-procedures\011_audit_procedures.sql
