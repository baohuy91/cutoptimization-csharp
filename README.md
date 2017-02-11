# CutOptimization
Solving alu bar cut as Cutting Stock Problem (CSP) with Lpsolve in C#
Can work on MacOs (need import modification) or Window
## Main Function Usage
Call `calRequiredBarWithFormat(...)` for get string formated patterns
_e.g._

```
string[]
{
    "1000 40 1v 600 + 1v 200"
    "1000 10 1v 600 + 1v 200 + 1v 100"
}
```

Call `calRequiredBar(...)` to get total number of required bar
Call `calRequiredBarCore(...)` to get dictionary `Dictionary<List<BarSet>, int>`. Which is map of Patterns number of reuquired stock for each pattern.

**NOTE:** Each func re-calculates everything, we just need to use ONE suitable func.

## Switch OS build by import modification
This project come with build for **Winx64 ddl**
To build with MacOs, in `ColumnGenerationSolver.cs`, change using namespace to

```
// using "lpsolve_win"
using "lpsolve_macos"
```

This is just change the wrapper lib for .dll to .dylib on mac. 2 lib lpsolve55.dll & liblpsolve55.dylib is attached with src.

in case we need to build for **Winx86** or other OS, download built lpsolve55.dll from [lpsolve download page](https://sourceforge.net/p/lpsolve/activity) package _lp_solve_5.5.2.5_dev_win32.zip_ etc.

## Unit Test
Source code is tested under dotnet core and tested under MacOs.

```
cd test/CutOptimization.Tests
dotnet test
```