fn Main() {
    Second();
    Third();
    Second();
}

fn Second() {
    Third();
}

fn Third() {
}
