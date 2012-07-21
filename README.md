Introduction
============
BulkImport a .NET library that makes it eaiser to bulk import CSV files into Microsoft SQL Server

Supported CSV
-------------
While there are various specifications and implementations for the CSV format, there is no formal 
specification in existence, which allows for a wide variety of interpretations of CSV files. 
BulkImport supports CSV files that follow the following format.


	1.	Each record is located on a separate line, delimited by a line break (CRLF).
		```
		"123","abc","1.49","12/25/2012"CRLF
		"456","def","9.19","04/08/2012"CRLF
		```
	2.	Every column MUST be surrounded by quotes ("). Spaces are not allowed ou.tside of quoted data
		```
		CORRECT:   "123","abc","1.49","12/25/2012"CRLF
		INCORRECT: 123,"abc",1.49,12/25/2012CRLF

		CORRECT:     "123","abc","1.49","12/25/2012"CRLF
		INCORRECT:   "123", "abc", "1.49", "12/25/2012" CRLF
		```
	3.	There maybe an optional header line appearing as the first line of the file with the same format 
		as normal record lines.  This header will contain names corresponding to the fields in the
		fileand should contain the same number of fields as the records in the rest of the file.
		```
		"Id","Name","Amount","Date"CRLF
		"123","abc","1.49","12/25/2012"CRLF
		"456","def","9.19","04/08/2012"CRLF
		```
Usage
-----
```c#
// Define table schema
var schema = new
{
	TransactionId = typeof(long),
	AccountId = typeof(int),
	FundName = typeof(string),
	Date = typeof(DateTime),
	Amount = typeof(decimal)
};
// fetch or load the CSV as a stream
var stream = GetOrLoadCsvStream();
var bi = new BulkImporter
			{
				ConnectionString = connectionString,
				TableName = tableName,
				Schema = schema,
				DataStream = stream
			};
bi.Execute();
```