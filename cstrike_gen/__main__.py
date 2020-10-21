import argparse, sys

from cstrike_gen import process_tiles 

def main():
    parser = argparse.ArgumentParser()
    subparsers = parser.add_subparsers(help="commands", dest="command")

    process_tiles_parser = subparsers.add_parser("process-tiles", aliases=["pt"])
    process_tiles_parser.add_argument("folder_path")

    args = parser.parse_args()
    print(dir(args))

    if args.command == "process-tiles":
        
        process_tiles.process_tiles(args.folder_path)

if __name__ == "__main__":
    main()