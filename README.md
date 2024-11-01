# delivery_filter

A CLI tool to read specific CSV format and filter entries by city district and datetime.

## CSV File Format

The tool expects input files to be in the following format, where each column represents specific data:
```csv
Id,District,Datetime,Weight
1,North,2023-10-01 08:30,15.4
2,East,2023-10-01 09:45,12.2
...

- **Id**: Unique identifier for the delivery
- **District**: Delivery region (any string)
- **Datetime**: Date and time of delivery in `yyyy-MM-dd HH:mm:ss` format
- **Weight**: Weight of the delivery in kg

## Installation

You'll need .NET installed.

```bash
$ dotnet restore
$ dotnet build
```

## Usage

```bash
$ ./delivery_filter get --help
$ ./delivery_filter get <file> [options]
```
#### Arguments and options

- `<file>`: Path to the CSV file with delivery data.
- `--district <district>`: Filter deliveries by district name (case-insensitive).
- `--from-datetime <from-datetime>`: Start date and time for filtering (format: `yyyy-MM-dd HH:mm:ss`). Default: `0001-01-01 00:00:00`.
- `--to-datetime <to-datetime>`: End date and time for filtering (format: `yyyy-MM-dd HH:mm:ss`). Default: `9999-12-31 23:59:59`.
- `--logfile <logfile>`: Path to a log file where logs will be saved.
- `--outfile <outfile>`: Path to the output CSV file to save filtered data.

### Example Usage

Filter deliveries from the `deliveries.csv` file, for the `North` district between specific dates, and save results to `filtered.csv`:

```bash
$ dotnet run -- get example.csv --district North --from-datetime 2023-10-01 00:00 --to-datetime 2023-10-31 23:59 --outfile filtered.csv
```

## Logging

Logs are managed by `log4net`. If a logfile path is specified with `--logfile`, the program logs all actions and errors to that file.