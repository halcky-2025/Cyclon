This repository is Cyclon, a programming language.
It is named after the C language with the aim of becoming the eye of the typhoon.

'''sample
model Hello{|
  String name
  int count
  void setCount{var c|
    count = c
  }
  void aisatu{|
    for {var i = 0, i < 9, i = i + 1|
      print(name)
    }
  }
}
model.Store(Hello.new())
var hs = model.Hello.Select()
server{|
  signal mousedown{var req, var res|
    var mouse = req.MouseDown.First()
    return ~<div log, #background "red"|
              <+div log0001| log1 updated>
            >~~
  }
  return ~<div log, #onclick mousedown|
            <div log0001, #b "white"| log1>
            <div log0002, #b "white"| log2>
          >~~
}
```

This programming language has a simple structure, with about 230 lines of parsing.
