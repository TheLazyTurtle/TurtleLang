fn Main() {
    Second(10);
}

fn Second(i: i32) {
    Third(i);
}

fn Third(i: i32) {
    for (i = 0; i < 100000; i++) {
        Print(i);
    }
}
