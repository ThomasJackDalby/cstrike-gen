class VmfParser:
    def __init__(self):
        self.c = ""

    def parse(self, file_path):
        with open(file_path) as f:
            self.f = f
            self.__read()
            return self.__parse_class_data()

    def __read(self):
        self.c = self.f.read(1)

    def __read_until(self, end_char, ignore_chars=[]):
        value = ""
        while self.c != end_char:
            if self.c not in ignore_chars:
                value += self.c
            self.__read()
        self.__read()
        return value

    def __skip_white_space(self):
        while self.c in [" ","\n","\r","\t"]:
            self.__read()

    def __parse_class_name(self):
        class_name = self.__read_until("{", [" ","\n","\r","\t"])
        self.__read()
        return class_name

    def __parse_class_data(self):
        class_data = {}
        while True:
            self.__skip_white_space()
            if self.c == "\"":
                self.__read()
                name, value = self.__parse_property()
                class_data[name] = value
            elif self.c == "}" or not self.c:
                self.__read()
                return class_data    
            else:
                name, value = self.__parse_class()
                if name not in class_data:
                    class_data[name] = []
                class_data[name].append(value)

    def __parse_class(self):
        class_name = self.__parse_class_name()
        self.__skip_white_space()
        class_data = self.__parse_class_data()
        return class_name, class_data

    def __parse_property(self):
        property_name = self.__read_until("\"")
        self.__read()
        self.__skip_white_space()
        self.__read()
        property_value = self.__read_until("\"")
        self.__read()
        return property_name, property_value

if __name__ == "__main__":
    vmf_parser = VmfParser()
    data = vmf_parser.parse("test_block.vmf")
    print(data)