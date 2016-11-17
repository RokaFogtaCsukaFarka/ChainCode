# Chain code
University Project of C#

This project is about generating a chain-code from a text file given
interpretation of a patch. The patch shouldn't have any boarders on the 1st row&column and 
last row&column.
It shall look like:
5 15
...............
.xxxx...xxxx...
..xxxxxxxx.....
....xxxx.xxxx..
...............

The first row contains the number of rows and columns in the matrix.
The program checks whether the patch is consistent, if the file has more subdivided patches in it.
Also it gives an error message if the numbers on the top does not match with the really existing value.
The chain-code traversing is of a clockwise manner. It has eight directions starting from the top,
which is No. 1 and the numbers are inscreasing anti-clockwise.

There are some test files provided in the /bin/Debug/fajlok directory.

