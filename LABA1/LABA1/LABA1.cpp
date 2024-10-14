
#include "framework.h"
#include "LABA1.h"
#include <map>
#include <cmath>
#include <tchar.h>

#define MAX_LOADSTRING 100
#define PI 3.14159265359

static std::map<HWND, bool> wins; 
POINT pt;                         
RECT rcClient;                    
HINSTANCE hInst;                  
WCHAR szTitle[MAX_LOADSTRING];    
WCHAR szWindowClass[MAX_LOADSTRING]; 
bool displayCoordsInBothWindows = false; 

#define IDM_TOGGLE_COORDS 32771

ATOM                MyRegisterClass(HINSTANCE hInstance);
BOOL                InitInstance(HINSTANCE, int);
LRESULT CALLBACK    WndProc(HWND, UINT, WPARAM, LPARAM);
INT_PTR CALLBACK    About(HWND, UINT, WPARAM, LPARAM);

void DrawClock(HDC hdc, RECT rcClient, bool isColorVariant);

int APIENTRY wWinMain(_In_ HINSTANCE hInstance,
    _In_opt_ HINSTANCE hPrevInstance,
    _In_ LPWSTR    lpCmdLine,
    _In_ int       nCmdShow)
{
    UNREFERENCED_PARAMETER(hPrevInstance);
    UNREFERENCED_PARAMETER(lpCmdLine);

    LoadStringW(hInstance, IDS_APP_TITLE, szTitle, MAX_LOADSTRING);
    LoadStringW(hInstance, IDC_LABA1, szWindowClass, MAX_LOADSTRING);
    MyRegisterClass(hInstance);

    if (!InitInstance(hInstance, nCmdShow))
    {
        return FALSE;
    }

    HACCEL hAccelTable = LoadAccelerators(hInstance, MAKEINTRESOURCE(IDC_LABA1));

    MSG msg;

    while (GetMessage(&msg, nullptr, 0, 0))
    {
        if (!TranslateAccelerator(msg.hwnd, hAccelTable, &msg))
        {
            TranslateMessage(&msg);
            DispatchMessage(&msg);
        }
    }

    return (int)msg.wParam;
}

//
//  ФУНКЦИЯ: MyRegisterClass()
//
ATOM MyRegisterClass(HINSTANCE hInstance)
{
    WNDCLASSEXW wcex;

    wcex.cbSize = sizeof(WNDCLASSEX);

    wcex.style = CS_HREDRAW | CS_VREDRAW;
    wcex.lpfnWndProc = WndProc;
    wcex.cbClsExtra = 0;
    wcex.cbWndExtra = 0;
    wcex.hInstance = hInstance;
    wcex.hIcon = LoadIcon(hInstance, MAKEINTRESOURCE(IDI_LABA1));
    wcex.hCursor = LoadCursor(nullptr, IDC_CROSS);
    wcex.hbrBackground = (HBRUSH)(COLOR_WINDOW + 1);
    wcex.lpszMenuName = MAKEINTRESOURCEW(IDC_LABA1);
    wcex.lpszClassName = szWindowClass;
    wcex.hIconSm = LoadIcon(wcex.hInstance, MAKEINTRESOURCE(IDI_SMALL));

    return RegisterClassExW(&wcex);
}

//
//   ФУНКЦИЯ: InitInstance(HINSTANCE, int)
//
BOOL InitInstance(HINSTANCE hInstance, int nCmdShow)
{
    hInst = hInstance; 

    HWND hWnd = CreateWindowW(szWindowClass, szTitle, WS_OVERLAPPEDWINDOW,
        CW_USEDEFAULT, 0, 500, 500, nullptr, nullptr, hInstance, nullptr);
    HWND hWnd2 = CreateWindowW(szWindowClass, szTitle, WS_OVERLAPPEDWINDOW,
        CW_USEDEFAULT, 0, 500, 500, nullptr, nullptr, hInstance, nullptr);

    if (!hWnd || !hWnd2)
    {
        return FALSE;
    }


    wins[hWnd] = true;  // Первое окно
    wins[hWnd2] = false; // Второе окно

    ShowWindow(hWnd, nCmdShow);
    ShowWindow(hWnd2, nCmdShow);
    UpdateWindow(hWnd);
    UpdateWindow(hWnd2);

    // Таймер с частотой обновления
    SetTimer(hWnd, 1, 1, NULL);
    SetTimer(hWnd2, 1, 1, NULL);

    return TRUE;
}

//
//  ФУНКЦИЯ: WndProc(HWND, UINT, WPARAM, LPARAM)
//
LRESULT CALLBACK WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
    switch (message)
    {
    case WM_COMMAND:
    {
        int wmId = LOWORD(wParam);
        switch (wmId)
        {
        case IDM_TOGGLE_COORDS:
            displayCoordsInBothWindows = !displayCoordsInBothWindows;
            break;
        case IDM_ABOUT:
            DialogBox(hInst, MAKEINTRESOURCE(IDD_ABOUTBOX), hWnd, About);
            break;
        case IDM_EXIT:
            DestroyWindow(hWnd);
            break;
        default:
            return DefWindowProc(hWnd, message, wParam, lParam);
        }
    }
    break;
    case WM_TIMER:
    {
        InvalidateRect(hWnd, NULL, TRUE); 
    }
    break;
    case WM_SIZE:
    {
        InvalidateRect(hWnd, NULL, TRUE); 
    }
    break;
    case WM_PAINT:
    {
        PAINTSTRUCT ps;
        HDC hdc = BeginPaint(hWnd, &ps);

        GetClientRect(hWnd, &rcClient);

        DrawClock(hdc, rcClient, wins[hWnd]);

        if (displayCoordsInBothWindows || GetActiveWindow() == hWnd)
        {
            TCHAR coords[50];
            _stprintf_s(coords, _T("X: %d, Y: %d"), pt.x, pt.y);
            TextOut(hdc, 10, 10, coords, _tcslen(coords));
        }

        EndPaint(hWnd, &ps);
    }
    break;
    case WM_MOUSEMOVE:
    {
        pt.x = LOWORD(lParam);
        pt.y = HIWORD(lParam);

        if (displayCoordsInBothWindows || GetActiveWindow() == hWnd)
        {
            InvalidateRect(hWnd, NULL, FALSE);
        }
    }
    break;
    case WM_LBUTTONDOWN:
    {
        wins[hWnd] = !wins[hWnd];
        InvalidateRect(hWnd, NULL, TRUE);
    }
    break;
    case WM_KEYDOWN:
    {
        if (wParam == 'T') 
        {
            displayCoordsInBothWindows = !displayCoordsInBothWindows;
        }
    }
    break;
    case WM_DESTROY:
    {
        KillTimer(hWnd, 1);
        PostQuitMessage(0);
    }
    break;
    default:
        return DefWindowProc(hWnd, message, wParam, lParam);
    }
    return 0;
}

void DrawClock(HDC hdc, RECT rcClient, bool isColorVariant)
{
    int width = rcClient.right - rcClient.left;
    int height = rcClient.bottom - rcClient.top;
    int radius = min(width, height) / 2 - 20;
    int centerX = rcClient.left + width / 2;
    int centerY = rcClient.top + height / 2;


    COLORREF clockColor = isColorVariant ? RGB(255, 0, 0) : RGB(0, 255, 0);


    HPEN hPen = CreatePen(PS_SOLID, 2, clockColor);
    HBRUSH hBrush = CreateSolidBrush(RGB(255, 255, 255));
    SelectObject(hdc, hPen);
    SelectObject(hdc, hBrush);
    Ellipse(hdc, centerX - radius, centerY - radius, centerX + radius, centerY + radius);
    DeleteObject(hBrush);

    SYSTEMTIME st;
    GetLocalTime(&st);

    double angleHour = (st.wHour % 12 + st.wMinute / 60.0) * 30.0; 
    double angleMinute = (st.wMinute + st.wSecond / 60.0) * 6.0;    
    double angleSecond = st.wSecond * 6.0;                          

    double radHour = (PI / 180.0) * angleHour;
    double radMinute = (PI / 180.0) * angleMinute;
    double radSecond = (PI / 180.0) * angleSecond;

    int lenHour = radius * 0.5;
    int lenMinute = radius * 0.7;
    int lenSecond = radius * 0.9;

    MoveToEx(hdc, centerX, centerY, NULL);
    LineTo(hdc, centerX + (int)(lenHour * sin(radHour)), centerY - (int)(lenHour * cos(radHour)));

    MoveToEx(hdc, centerX, centerY, NULL);
    LineTo(hdc, centerX + (int)(lenMinute * sin(radMinute)), centerY - (int)(lenMinute * cos(radMinute)));

    MoveToEx(hdc, centerX, centerY, NULL);
    LineTo(hdc, centerX + (int)(lenSecond * sin(radSecond)), centerY - (int)(lenSecond * cos(radSecond)));

    DeleteObject(hPen);
}
// Обработчик сообщений для окна "О программе".
INT_PTR CALLBACK About(HWND hDlg, UINT message, WPARAM wParam, LPARAM lParam)
{
    UNREFERENCED_PARAMETER(lParam);
    switch (message)
    {
    case WM_INITDIALOG:
        return (INT_PTR)TRUE;
    case WM_COMMAND:
        if (LOWORD(wParam) == IDOK || LOWORD(wParam) == IDCANCEL)
        {
            EndDialog(hDlg, LOWORD(wParam));
            return (INT_PTR)TRUE;
        }
        break;
    }
    return (INT_PTR)FALSE;
}
