This is a project done for the master thesis on the Faculty of Mathematics, University of Belgrade.
The title of the master thesis is 'Horizontal Scaling of Database Management Systems (DBMS)'.

The main idea of the project is to create a prototype that would highlight how Azure SQL DB HyperScale (https://docs.microsoft.com/en-us/azure/azure-sql/database/service-tier-hyperscale) achieves consistency over multiple processes that represent the database.


Traditional DBMS has been implemented so far with an easy way of testing for a process crash. Couple of tests are implemented.

Syntax:
CREATE TABLE <table_name>

INSTER INTO <table_name> VALUES
    <integer> [, <integer>]
  | <empty>

CHECK <table_name> VALUES
    <integer> [, <integer>]
  | <empty>

KILL
