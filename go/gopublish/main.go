package main

import (
	"encoding/json"
	"encoding/xml"
	"io/fs"
	"io/ioutil"
	"os"
	"path/filepath"
	"strings"
)

// describes json output
type UnitTest struct {
	TestName string `json:"test_name,omitempty"`
	Outcome  string `json:"outcome,omitempty"`
	Output   string `json:"output,omitempty"`
	Error    string `json:"error,omitempty"`
	Stack    string `json:"stack,omitempty"`
}
type TestResults struct {
	Name string     `json:"name"`
	File []string   `json:"file"`
	Test []UnitTest `json:"test"`
}

// extensions split by |
func IndexFiles(input string, extension string) []string {
	extv := strings.Split(extension, "|")
	exts := map[string]bool{}
	for _, o := range extv {
		exts[o] = true
	}
	r := []string{}
	matchGo := func(p string, d fs.DirEntry, err error) error {
		ext := filepath.Ext(p)
		if exts[ext] {
			r = append(r, p)
		}
		return nil
	}
	fsys := os.DirFS(input)
	fs.WalkDir(fsys, ".", matchGo)
	return r
}

// this walks pth and writes the selected files (for now just jpg) to a json that is just string[]

func Move(from, to string) {
	os.RemoveAll(to)
	os.Rename(from, to)
}

func copy(source, destination string) error {

	var err error = filepath.Walk(source, func(path string, info os.FileInfo, err error) error {
		var relPath string = strings.Replace(path, source, "", 1)
		if relPath == "" {
			return nil
		}
		if info.IsDir() {
			return os.Mkdir(filepath.Join(destination, relPath), 0755)
		} else {
			var data, err1 = ioutil.ReadFile(filepath.Join(source, relPath))
			if err1 != nil {
				return err1
			}
			err = ioutil.WriteFile(filepath.Join(destination, relPath), data, 0666)
			if err != nil {
				panic(err)
			}
			return nil
		}
	})
	if err != nil {
		panic(err)
	}
	return err
}
func Copy(from, to string) {
	os.RemoveAll(to)
	os.Mkdir(to, os.ModePerm)
	copy(from, to)
}
func TrxResults(path string) []UnitTestResult {
	var t TestRun
	b, e := os.ReadFile(path)
	if e != nil {
		panic(e)
	}
	e = xml.Unmarshal(b, &t)
	if e != nil {
		panic(e)
	}
	return t.Results.UnitTestResult
}

func Process(name string, dir string) {

	var t TestResults
	t.Name = name
	t.File = IndexFiles(dir, ".jpg")
	for _, x := range TrxResults(dir + "/test_results.trx") {
		var o UnitTest
		o.TestName = x.TestName
		o.Outcome = x.Outcome
		o.Output = x.Output.StdOut
		o.Error = x.Output.ErrorInfo.Message
		o.Stack = x.Output.ErrorInfo.StackTrace
		t.Test = append(t.Test, o)
	}

	b, err := json.Marshal(&t)
	if err != nil {
		panic(err)
	}
	os.WriteFile(dir+"/../test_results.json", b, 0666)

}
func Nightly(name string) {
	Move("../../test/imis/TestResults", "../results/public/TestResults")
	Copy("../../test/imis/gherkin", "../results/public/gherkin")
	Process(name, "../results/public/TestResults")
}
func main() {
	if len(os.Args) > 1 {
		Nightly(os.Args[1])
	} else {
		Nightly("v10")
	}
}
