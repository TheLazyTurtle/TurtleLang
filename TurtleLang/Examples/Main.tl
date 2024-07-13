
struct CustomType {
    i: i32;
    str: string;
}

impl CustomType {
    fn print_values(self) {
        Print(self.i);
        Print(self.str);
    }
}

fn Main() {
    var custom: CustomType = new CustomType {
        i = 10,
        str = "asdf",
    };
    second(custom);
}

fn second(s: CustomType) {
    s.print_values();
}