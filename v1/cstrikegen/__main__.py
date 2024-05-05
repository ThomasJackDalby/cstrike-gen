import argparse, sys

def main():
    parser = argparse.ArgumentParser()
    subparsers = parser.add_subparsers(help="commands", dest="command")

    process_tiles_parser = subparsers.add_parser("process-tiles", aliases=["pt"])
    process_tiles_parser.add_argument("folder_path")

    args = parser.parse_args()
    print(dir(args))

if __name__ == "__main__":
    main()