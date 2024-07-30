struct CustomType {
    i: i32;
    str: string;
}

trait Printable {
    fn print(self) : void;
    fn return_int(self) : i32;
}

impl CustomType {
    fn print_values(self) : void {
        Print(self.i);
        Print(self.str);
    }
}

impl Printable for CustomType {
    fn print(self) : void {
        self.print_values();
    }
    
    fn return_int(self) : i32 {
        return 1;
    }
}

fn Main() : void {
    var custom: CustomType = new CustomType {
        i = 10,
        str = "asdf",
    };
    second(custom);
}

fn second(s: CustomType): void {
    s.print_values();
    s.print();
    var ret_val : i32 = s.return_int();
    Print(ret_val);
}