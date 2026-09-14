import sys
import os
'''
arcaea谱面转换为sprintin谱面
tap和hold分别一一对应
singleArcTap对应ground
'''

def transform(content):
    ret = ""
    lines = content.splitlines()
    for line in lines:
        cur = ""
        if line.startswith("timing("):
            tmp = line[7:-2].split(",")
            cur = f"({tmp[0]}, {tmp[1]}, {tmp[2]});\n"
        elif line.startswith("hold("):
            tmp = line[5:-2].split(",")
            cur = f"[{tmp[0]}, {tmp[2]}, {tmp[1]}];\n"
        elif line.startswith("arc("):
            tmp = line[4:-2].split(",")
            cur = f"[{tmp[0]}, 0];\n"
        elif line.startswith("("):
            tmp = line[1:-2].split(",")
            cur = f"[{tmp[0]}, {tmp[1]}];\n"
        ret += cur
    return ret

def main():
    if len(sys.argv) < 2:
        print("Usage: python FromAff.py <input_file> [output_file]")
        print("  input_file:  path to the input file")
        print("  output_file: path to the output file (default: ./0.spr)")
        sys.exit(1)

    input_path = sys.argv[1]
    output_path = sys.argv[2] if len(sys.argv) > 2 else "./0.spr"

    if not os.path.exists(input_path):
        print(f"Error: input file not found: {input_path}")
        sys.exit(1)

    with open(input_path, "r", encoding="utf-8") as f:
        content = f.read()
    content = transform(content)
    
    os.makedirs(os.path.dirname(os.path.abspath(output_path)), exist_ok=True)

    with open(output_path, "w", encoding="utf-8") as f:
        f.write(content)

    print(f"Successfully wrote {len(content)} characters to {output_path}")


if __name__ == "__main__":
    main()