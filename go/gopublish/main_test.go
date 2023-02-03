package main

import "testing"

func Test_one(t *testing.T) {
	Nightly("v10")
}

func Test_two(t *testing.T) {

	Process("V10", "../results/public/TestResults")
}

func Test_copy(t *testing.T) {

	Copy("../../test/imis/gherkin", "../results/public/gherkin")
}
