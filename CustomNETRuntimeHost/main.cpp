#include <windows.h>
#include <stdio.h>
#include <string>
#include <fstream>
#include <nethost.h>
#include <coreclr_delegates.h>
#include <hostfxr.h>
#include <objbase.h> // Add this for COM functions

#pragma comment(lib, "user32.lib")
#pragma comment(lib, "ole32.lib") // Add this for COM functions

#define STR(s) L ## s
#define CH(c) L ## c
#define DIR_SEPARATOR L'\\'

using string_t = std::basic_string<char_t>;
std::atomic<bool> g_threadRunning(false);
std::ofstream logFile;

void LogMessage(const std::string& message) {
    if (!logFile.is_open()) {
        logFile.open("C:\\Temp\\DllLog.txt", std::ios::app);
    }
    logFile << message << std::endl;
    printf("%s\n", message.c_str());
    fflush(stdout);
}

inline std::string to_string(const wchar_t* wstr) {
    if (!wstr) return "";
    std::string str;
    while (*wstr) str += static_cast<char>(*wstr++);
    return str;
}

// Function pointers for .NET hosting API
hostfxr_initialize_for_runtime_config_fn init_fptr;
hostfxr_get_runtime_delegate_fn get_delegate_fptr;
hostfxr_close_fn close_fptr;

bool load_hostfxr()
{
    // Load hostfxr and get desired exports
    HMODULE lib = LoadLibraryW(SOLUTION_DIR L"output2\\hostfxr.dll");
    if (lib == NULL)
    {
        LogMessage("Failed to load hostfxr.dll. Error code: " + std::to_string(GetLastError()));
        return false;
    }


    init_fptr = (hostfxr_initialize_for_runtime_config_fn)GetProcAddress(lib, "hostfxr_initialize_for_runtime_config");
    get_delegate_fptr = (hostfxr_get_runtime_delegate_fn)GetProcAddress(lib, "hostfxr_get_runtime_delegate");
    close_fptr = (hostfxr_close_fn)GetProcAddress(lib, "hostfxr_close");

    if (!init_fptr || !get_delegate_fptr || !close_fptr)
    {
        LogMessage("Failed to get function pointers from hostfxr.dll");
        FreeLibrary(lib);
        return false;
    }

    //LogMessage("hostfxr.dll loaded successfully");
    return true;
}

bool load_and_run()
{
    // Initialize COM
    HRESULT hr = CoInitializeEx(NULL, COINIT_APARTMENTTHREADED | COINIT_DISABLE_OLE1DDE);
    if (FAILED(hr))
    {
        LogMessage("Failed to initialize COM. HRESULT: " + std::to_string(hr));
        return false;
    }
    //LogMessage("COM initialized successfully");

    // Load .NET Core
    if (!load_hostfxr())
    {
        CoUninitialize();
        LogMessage("Failed to load hostfxr");
        return false;
    }
    //LogMessage("hostfxr loaded successfully");

    // Get the path to the .NET Core runtime configuration file
    const wchar_t* config_path = SOLUTION_DIR L"output2\\TestCS.runtimeconfig.json";
    
    // Load and initialize .NET Core
    void* load_assembly_and_get_function_pointer = nullptr;
    hostfxr_handle cxt = nullptr;
    int rc = init_fptr(config_path, nullptr, &cxt);
    if (rc != 0 || cxt == nullptr)
    {
        LogMessage("Init failed: " + std::to_string(rc));
        close_fptr(cxt);
        return false;
    }

    // Get the load assembly function pointer
    rc = get_delegate_fptr(
        cxt,
        hdt_load_assembly_and_get_function_pointer,
        &load_assembly_and_get_function_pointer);
    if (rc != 0 || load_assembly_and_get_function_pointer == nullptr)
    {
        LogMessage("Get delegate failed: " + std::to_string(rc));
        close_fptr(cxt);
        return false;
    }

    //LogMessage(".NET Core runtime initialized successfully");

    // Load managed assembly and get function pointer to a managed method
    const char_t* dotnetlib_path = SOLUTION_DIR STR("output2\\TestCS.dll");
    const char_t* type_name = STR("TestCS.Program, TestCS");
    const char_t* method_name = STR("Main");
    void* delegate = nullptr;

    //LogMessage("Attempting to load: " + to_string(dotnetlib_path));
    //LogMessage("Type name: " + to_string(type_name));
    //LogMessage("Method name: " + to_string(method_name));

    rc = ((load_assembly_and_get_function_pointer_fn)load_assembly_and_get_function_pointer)(
        dotnetlib_path,
        type_name,
        method_name,
        UNMANAGEDCALLERSONLY_METHOD,
        nullptr,
        &delegate);

    if (rc != 0 || delegate == nullptr)
    {
        LogMessage("Failed to load assembly and get function pointer. Error code: " + std::to_string(rc));
        // TODO: Add more detailed error reporting here if possible
        close_fptr(cxt);
        return false;
    }

    //LogMessage("Assembly loaded and function pointer obtained successfully");

    // Call managed function
    ((void(*)())delegate)();

    //LogMessage("Managed function called successfully");

    close_fptr(cxt);
    CoUninitialize();
    return true;
}

DWORD WINAPI ManagedCodeThread(LPVOID lpParam) {
    try {
        if (!load_and_run()) {
            LogMessage("Failed to execute managed code");
        }
    }
    catch (const std::exception& e) {
        LogMessage("Exception occurred: " + std::string(e.what()));
    }
    catch (...) {
        LogMessage("Unknown exception occurred");
    }

    g_threadRunning = false;
    return 0;
}

BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved) {
    switch (ul_reason_for_call) {
    case DLL_PROCESS_ATTACH:
        AllocConsole();
        FILE* f;
        freopen_s(&f, "CONOUT$", "w", stdout);
        LogMessage("DLL_PROCESS_ATTACH reached");

        // Create a new thread to run the managed code
        g_threadRunning = true;
        CreateThread(NULL, 0, ManagedCodeThread, NULL, 0, NULL);
        break;

    case DLL_PROCESS_DETACH:
        LogMessage("DLL_PROCESS_DETACH reached");

        // Wait for the thread to finish (with a timeout)
        DWORD startTime = GetTickCount64();
        while (g_threadRunning && (GetTickCount64() - startTime < 5000)) {
            Sleep(100);
        }

        if (logFile.is_open()) {
            logFile.close();
        }
        FreeConsole();
        break;
    }
    return TRUE;
}

//extern "C" __declspec(dllexport) void DummyFunction() { }
