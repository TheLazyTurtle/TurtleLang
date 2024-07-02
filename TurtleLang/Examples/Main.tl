struct customType {
    int: i32;
    str: string;
}

fn Main() {
    var i: string = "asdf";
    i = "jkl;";
    Second(i);
}

fn Second(i: string) {
    for (var j = 0; j < 5; j++) {
        Print(j);
    }
    
    var k: i32 = 0;
    if (k == 0) {
        Print("if still works");
    }
    Print(i);
}
