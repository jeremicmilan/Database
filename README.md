# Database

This is a project done for the master thesis for the Faculty of Mathematics, University of Belgrade.
The title of the master thesis is 'Horizontal Scaling of Database Management Systems (DBMS)'.

The main idea of the project is to create a prototype that would highlight how Azure SQL DB HyperScale (https://docs.microsoft.com/en-us/azure/azure-sql/database/service-tier-hyperscale) achieves consistency over multiple processes that represent the database.

## Setup

Downalod and install latest Visual Studio (i.e. VS22). Intall all needed VS redistributables (I can't remember all the things I installed, so hopefully VS prompts you to what's missing).

Open VS and pull the code from github.

Recommended, but not mandatory: Before building and running the code go to "Tools -> Options -> Debugging -> General" and turn on the option "Automatically close the console when debugging stops".

## Syntax

<pre><code>CREATE TABLE <<a>table_name>

INSERT INTO <<a>table_name> VALUES <<a>integer> [, <<a>integer>]

DELETE FROM <<a>table_name> VALUES <<a>integer> [, <<a>integer>]

SELECT FROM <<a>table_name>

CHECK <<a>table_name> VALUES
    <<a>integer> [, <<a>integer>]
  | <<a>empty>

CHECKPOINT

BEGIN TRANSACTION
END TRANSACTION

KILL

RUN <<a>test_name>
RUN ALL

CONFIGURE LOGGING OFF
CONFIGURE LOGGING ON

CONFIGURE DATABASE TRADITIONAL
CONFIGURE DATABASE HYPERSCALE</code></pre>
