import subprocess
import tempfile
import csv
import os
from datetime import datetime

test_data = """Id,District,Datetime,Weight
1,North,2023-10-01 08:30,15.4
2,East,2023-10-01 09:45,12.2
3,West,2023-10-01 10:15,18.7
4,South,2023-10-01 11:00,22.5
5,North,2023-10-01 11:30,16.3
6,East,2023-10-01 12:45,10.0
"""

def run_cli_command(*args):
    result = subprocess.run(
        ["dotnet", "run", "--"] + list(args),
        capture_output=True, text=True, check=True
    )
    return result.stdout

# --- Filter by District
def test_filter_by_district():
    with tempfile.NamedTemporaryFile(mode='w+', delete=False, suffix='.csv') as temp_infile:
        temp_infile.write(test_data)
        temp_infile.flush()
        temp_outfile = tempfile.NamedTemporaryFile(delete=False, suffix='.csv')
        
        run_cli_command("get", temp_infile.name, "--district", "North", "--outfile", temp_outfile.name)
        
        with open(temp_outfile.name, newline='') as csvfile:
            reader = csv.DictReader(csvfile)
            rows = list(reader)
            assert len(rows) == 2
            assert rows[0]["District"] == "North"
            assert rows[1]["District"] == "North"

        os.remove(temp_outfile.name)
    os.remove(temp_infile.name)

# --- Filter by Date Range
def test_filter_by_date_range():
    with tempfile.NamedTemporaryFile(mode='w+', delete=False, suffix='.csv') as temp_infile:
        temp_infile.write(test_data)
        temp_infile.flush()
        temp_outfile = tempfile.NamedTemporaryFile(delete=False, suffix='.csv')
        
        run_cli_command(
            "get", temp_infile.name, 
            "--from-datetime", "2023-10-01 09:00", 
            "--to-datetime", "2023-10-01 12:00", 
            "--outfile", temp_outfile.name
        )

        with open(temp_outfile.name, newline='') as csvfile:
            reader = csv.DictReader(csvfile)
            rows = list(reader)
            assert len(rows) == 4
            for row in rows:
                assert datetime.strptime(row["Datetime"], "%Y-%m-%d %H:%M:%S") >= datetime(2023, 10, 1, 9, 0)
                assert datetime.strptime(row["Datetime"], "%Y-%m-%d %H:%M:%S") <= datetime(2023, 10, 1, 12, 0)

        os.remove(temp_outfile.name)
    os.remove(temp_infile.name)

# --- Missing Required Columns
def test_missing_columns():
    invalid_data = """Id,District,Weight
1,North,15.4
2,East,12.2
"""
    with tempfile.NamedTemporaryFile(mode='w+', delete=False, suffix='.csv') as temp_infile:
        temp_infile.write(invalid_data)
        temp_infile.flush()
        result = subprocess.run(
            ["dotnet", "run", "--", "get", temp_infile.name, "--district", "North"],
            capture_output=True, text=True
        )
        assert "CSV does not have required columns" in result.stdout
    os.remove(temp_infile.name)

# --- File Not Found
def test_file_not_found():
    result = subprocess.run(
        ["dotnet", "run", "--", "get", "nonexistent.csv", "--district", "North"],
        capture_output=True, text=True
    )
    assert "File 'nonexistent.csv' not found" in result.stdout

# Run tests
if __name__ == "__main__":
    test_filter_by_district()
    test_filter_by_date_range()
    test_missing_columns()
    test_file_not_found()
    print("All tests passed.")
