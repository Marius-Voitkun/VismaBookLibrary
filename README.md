# Visma Book Library
### Console application to manage Visma's book library.
<br>

**The following commands are available in the application:**<br>
\> add-book<br>
\> add-reader<br>
\> delete-book [*bookId*]<br>
\> delete-reader [*readerId*]<br>
\> list-books<br>
\> list-books -f-[*filteringProperty*] [*filteringValue*]<br>
\> list-readers<br>
\> return-book [*bookId*]<br>
\> take-book [*bookId*] [*readerId*] [*numberOfDays*]<br>
\> help<br>
\> exit<br>

*Available filters for* list-books *command:* -f-author, -f-category, -f-language, -f-ISBN, -f-name, -f-taken, -f-available.<br>
*The filters check if property values of books contain specified string (case does not matter).*<br>
*Example:*<br>
\> list-books -f-author "Andrew Hunt" -f-available -f-language English<br>
*Please note that only double-quotes are valid.*<br>
