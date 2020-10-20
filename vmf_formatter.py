class VmfFormatter:
    def __init__(self):
        self.pretty = True
        self.indent = 0

    def format(self, file_path, data):
        with open(file_path, "w") as f:
            self.f = f
            self.__format_class_data(data)

    def __format_property(self, property_name, property_value):
        if self.pretty:
            self.f.write("\t" * self.indent)

        self.f.write(f"\"{property_name}\"")
        self.f.write(" ")
        self.f.write(f"\"{property_value}\"")
        if self.pretty:
            self.f.write("\n")

    def __format_class(self, class_name, class_data):
        if self.pretty:
            self.f.write("\t" * self.indent)
        
        self.f.write(class_name)
        if self.pretty:
            self.f.write("\n")
        
        if self.pretty:
            self.f.write("\t" * self.indent)
        self.f.write("{")
        self.indent += 1
        if self.pretty:
            self.f.write("\n")

        self.__format_class_data(class_data)

        self.indent -= 1
        if self.pretty:
            self.f.write("\t" * self.indent)
        self.f.write("}")
        if self.pretty:
            self.f.write("\n")

    def __format_class_data(self, class_data):
        for key in class_data:
            value = class_data[key]
            if isinstance(value, list):
                for data in value:
                    self.__format_class(key, data)          
            else:
                self.__format_property(key, value)

if __name__ == "__main__":
    vmf_formatter = VmfFormatter()

    data = {
        "world" : [{
            "solid" : [
                {
                    "test" : "something else"
                },
                {
                    "test" : "something else 2"
                }
            ]
        }]
    }
    vmf_formatter.format("test.vmf", data)