fn Main() {
    Second(10);
}

fn Second(i) {
    if (i > 10) {
        Print("If branch", i);
    }
    else {
        Print("Else branch", i);
    }
    Print(i);
}
