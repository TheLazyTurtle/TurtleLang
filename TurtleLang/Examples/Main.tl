fn Main() {
    Second(1, 2);
}

fn Second(i, j) {
    Third(i, "asdf");
}

fn Third(varA, varB) {
    Print(varA, "This is a quick string in between", varB);
}
