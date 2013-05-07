CREATE DATABASE stocks_with_friends_database
GO

USE stocks_with_friends_database
GO

CREATE TABLE StockTransactions
(
id int NOT NULL PRIMARY KEY,
user_id bigint NOT NULL,
timestamp DATETIME NOT NULL,
stock_symbol varchar(255) NOT NULL,
tx_quantity_delta int NOT NULL,
tx_price float NOT NULL
)

CREATE TABLE StockNote
(
id int NOT NULL PRIMARY KEY,
user_id bigint NOT NULL,
stock_symbol varchar(255) NOT NULL,
note_string varchar(2047)
)

CREATE TABLE CalendarEvents
(
id int NOT NULL PRIMARY KEY,
user_id bigint NOT NULL,
event_name varchar(255),
event_description varchar(2047),
start_timestamp DATETIME NOT NULL,
end_timestamp DATETIME NOT NULL
)

CREATE TABLE ChatLogs
(
id int NOT NULL PRIMARY KEY,
user_id bigint NOT NULL,
timestamp DATETIME NOT NULL,
message varchar(2047)
)