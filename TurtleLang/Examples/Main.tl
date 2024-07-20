trait Printable {
    fn print(self);
}

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

impl Printable for CustomType {
    fn print(self) {
        self.print_values();
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
    if (s.i == 10) {
        return;
    }
    s.print_values();
    s.print();
}