import subprocess, sys, shutil, os

# 

def compile_map(file_path):
    subprocess.run(["vbsp", file_path], shell=True, check=True)
    subprocess.run(["vvis", file_path], shell=True, check=True)
    subprocess.run(["vrad", file_path], shell=True, check=True)

    bsp_file_path = os.path.splitext(file_path)[0] + ".bsp"
    shutil.copy(bsp_file_path, "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Counter-Strike Source\\cstrike\\maps")

if __name__ == "__main__":
    file_path = sys.argv[1]
    compile_map(file_path)