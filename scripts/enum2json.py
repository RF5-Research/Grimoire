import sys
import os
import json
import re

if "__main__" == __name__:
    file_path = sys.argv[1]
    output = dict()
    with open(file_path, "rt") as fs:
        while "{" not in fs.readline():
            pass
        while line := fs.readline():
            if line == "}":
                break
            tokens = [x for x in line.split("=")]
            if len(tokens) == 2:
                output[tokens[0].strip()] = int(re.search(r"\d+", tokens[1]).group())
    with open(f"{os.path.basename(file_path).split('.')[0]}.json", "wt") as writer:
        json.dump(output, writer, indent=4)