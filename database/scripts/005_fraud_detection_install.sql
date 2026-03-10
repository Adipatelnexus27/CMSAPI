-- Run with sqlcmd from CMSAPI/database/scripts:
-- sqlcmd -S "(LocalDb)\MSSQLLocalDB" -d "CMS" -i "005_fraud_detection_install.sql"

:r .\schema\006_fraud_detection_schema.sql
:r .\seeds\006_fraud_detection_seed.sql
:r .\stored-procedures\006_fraud_detection_procedures.sql
