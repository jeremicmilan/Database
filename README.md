# Database

This is a project done for the master thesis on the Faculty of Mathematics, University of Belgrade.
The title of the master thesis is 'Horizontal Scaling of Database Management Systems (DBMS)'.

The main idea of the project is to create a prototype that would highlight how Azure SQL DB HyperScale (https://docs.microsoft.com/en-us/azure/azure-sql/database/service-tier-hyperscale) achieves consistency over multiple processes that represent the database.

Traditional DBMS has been implemented so far with an easy way of testing for a process crash. Couple of tests are added as well.

## Syntax

<pre><code>CREATE TABLE <<a>table_name>

INSERT INTO <<a>table_name> VALUES
    <<a>integer> [, <<a>integer>]  
  | <<a>empty>

DELETE FROM <<a>table_name> VALUES
    <<a>integer> [, <<a>integer>]  
  | <<a>empty>

SELECT FROM <<a>table_name>  

CHECK <<a>table_name> VALUES  
    <<a>integer> [, <<a>integer>]  
  | <<a>empty>

CHECKPOINT

BEGIN TRANSACTION
END TRANSACTION

KILL

RUN <<a>test_name>
RUN ALL</code></pre>
