# SamsungAGV-UI

UI project to monitor the movement of Automated Guided Vehicle using Visual Studio Form Apps coded using C# programming language. We use Uvicorn as FastAPI for building APIs & web frameworks coded with Python.

### Proposed Design
![](Final_UI_Design.jpg)

### Built With

* [Uvicorn](https://www.uvicorn.org/) - Uvicorn FastAPI (Python Web Framework)
* [Visual Sttudio](https://visualstudio.microsoft.com/) - Visual Studio integrated development environment (IDE)
* [Bunifu Framework](https://bunifuframework.com/) - Bunifu Framework UI tools

### Prerequisites

Function dependencies used in this project:

- Python 3.9.13
- FastAPI 0.72.0
- Starlette 0.17.1
- Uvicorn 0.17.0
- Pydantic 1.8.2
- Requests 2.27.1


### Run Application
```
cd "<your directory>\.NEW_VERSION\agv\"
start AGVServer.bat
cd "<your directory>\.NEW_VERSION\agv\"
start webApi.bat
cd "<your directory>\.NEW_VERSION\"
start python backendPython.py
cd "<your directory>\.NEW_VERSION\bin\Debug"
start SampleUI-SamsungAGV.exe
```

change the `<your directory>` according to the directory where you save all the AGV files
