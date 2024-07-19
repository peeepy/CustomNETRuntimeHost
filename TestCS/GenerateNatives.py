import json
from typing import List, Dict

cpp_to_cs_type_map = {
    "int": "int",
    "float": "float",
    "bool": "bool",
    "const char*": "IntPtr",
    "Any*": "IntPtr",
    "void*": "IntPtr",
    "Void": "void",
    "BOOL": "BOOL",
    "Hash": "uint",
}

class Argument:
    def __init__(self, name: str, type: str):
        self.name = name.replace("...", "varargs")
        self.type = self.convert_type(type)
        self.cs_type = self.get_cs_type(type)
        self.is_string = type == "const char*"

    def convert_type(self, cpp_type: str) -> str:
        return cpp_to_cs_type_map.get(cpp_type, cpp_type)

    def get_cs_type(self, cpp_type: str) -> str:
        if cpp_type == "const char*":
            return "string"
        return "bool" if cpp_type == "BOOL" else self.convert_type(cpp_type)

class NativeFunction:
    def __init__(self, namespace: str, name: str, return_type: str, args: List[Dict]):
        self.namespace = namespace
        self.name = name
        self.return_type = self.convert_return_type(return_type)
        self.cs_return_type = self.get_cs_return_type(return_type)
        self.args = [Argument(arg['name'], arg['type']) for arg in args]
        self.is_bool_return = return_type == 'BOOL'
        self.is_string_return = return_type == 'const char*'

    def convert_return_type(self, cpp_type: str) -> str:
        return cpp_to_cs_type_map.get(cpp_type, cpp_type)

    def get_cs_return_type(self, cpp_type: str) -> str:
        if cpp_type == "const char*":
            return "string"
        return "bool" if cpp_type == "BOOL" else self.convert_return_type(cpp_type)

    def get_libraryimport_declaration(self) -> str:
        args = []
        for arg in self.args:
            if arg.is_string:
                args.append(f"[MarshalAs(UnmanagedType.LPStr)] string {arg.name}")
            else:
                args.append(f"{arg.type} {arg.name}")
        
        args_str = ", ".join(args)
        
        attributes = [
            'LibraryImport("RDONatives.dll")',
            'UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })'
        ]
        
        if self.is_string_return:
            attributes.append('\n        [return: MarshalAs(UnmanagedType.LPStr)')
        
        attributes_str = "[" + ", ".join(attributes) + "]"
        return f"{attributes_str}\nprivate static partial {self.return_type} {self.name}_Export({args_str});"

    def get_wrapper_function(self) -> str:
        args = ", ".join([f"{arg.cs_type} {arg.name}" for arg in self.args])
        params = ", ".join([f"{arg.name} ? 1 : 0" if arg.type == 'BOOL' else arg.name for arg in self.args])
        
        if self.is_bool_return:
            return f"""public static {self.cs_return_type} {self.name}({args})
    {{
        return {self.name}_Export({params}) != 0;
    }}"""
        elif self.is_string_return:
            return f"""public static {self.cs_return_type} {self.name}({args})
    {{
        var result = {self.name}_Export({params});
        return result == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(result);
    }}"""
        else:
            return_statement = f"return " if self.cs_return_type != "void" else ""
            return f"""public static {self.cs_return_type} {self.name}({args})
    {{
        {return_statement}{self.name}_Export({params});
    }}"""

def load_natives_data(file_path: str) -> Dict[str, List[NativeFunction]]:
    with open(file_path, 'r') as f:
        data = json.load(f)
    
    natives = {}
    for namespace, functions in data.items():
        natives[namespace] = []
        for hash_str, func_data in functions.items():
            natives[namespace].append(NativeFunction(
                namespace,
                func_data['name'],
                func_data['return_type'],
                func_data['params']
            ))
    return natives

def generate_csharp_code(natives: Dict[str, List[NativeFunction]]) -> str:
    code = """using System;
using System.Runtime.InteropServices;

namespace Natives
{
"""
    
    for namespace, functions in natives.items():
        code += f"    public static unsafe partial class {namespace}\n    {{\n"
        for func in functions:
            code += f"        {func.get_libraryimport_declaration()}\n\n"
            code += f"        {func.get_wrapper_function()}\n\n"
        code += "    }\n\n"
    
    code += "}"
    return code

def write_csharp_file(code: str, file_path: str):
    with open(file_path, 'w') as f:
        f.write(code)

if __name__ == "__main__":
    natives_data = load_natives_data("natives.json")
    csharp_code = generate_csharp_code(natives_data)
    write_csharp_file(csharp_code, "Natives.cs")
    print("C# code has been generated and written to Natives.cs")