#Retrieval Engine
<i>By Amir Fefer & Ido Rosenzwig </i><br> <br>
A Retrieval engine on large amount of text files, ordering by documents. <br>
This part includes only the Parsing & Indexing phase <br>
First, the text files parsed to terms, and then the engine index it, <br>
 and build posting files, and inverted files dictionary<br>

#Features 
* Parsing Docs to terms. <br>
* Terms can be numbers, range, dates, names, and so on. <br>
* The terms can be stemmed (GUI option) <br>
* Support in stop words.  <br>
* Loading inverted files dictionary, and sort it by name.  <br>
* Uses psaudo LRU algorithm on common terms.  <br>

#Text Files Format 
The engine support multiple text files, each text file can contain multiple Docs. <br>
 Each doc needs to be like the following format:<br>
 ```xml
<DOC><br>
	<DOCNO><br>
		Doc ID<br>
	</DOCNO><br>
	<HEADER><br>
		headline...<br>
	</HEADER><br>
	<TEXT><br>
		Document text<br>
	</TEXT><br>
</DOC><br>
```