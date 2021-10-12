# VismaBookLibrary
Console application to manage Visma's book library.<br>

The following commands are available:<br>
- add-book<br>
- add-reader<br>
- delete-book [bookId]<br>
- delete-reader [readerId]<br>
- list-books<br>
- list-books -f-[filteringProperty] [filteringValue]<br>
> Available filters: -f-author, -f-category, -f-language, -f-ISBN, -f-name, -f-taken, -f-available<br>
> e. g.:<br>
- list-books -f-author "Andrew Hunt" -f-available -f-language English<br>
> (please note that only double-quotes are valid)<br>
- list-readers<br>
- return-book [bookId]<br>
- take-book [bookId] [readerId] [numberOfDays]<br>
- help<br>
- exit<br>
