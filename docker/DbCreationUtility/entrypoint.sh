#!/bin/bash
flag_filename="/db-creator-data/flag"

if [ ! -f $flag_filename ]; then
    dotnet Avanpost.Interviews.Task.Integration.Data.DbCreationUtility.dll $1 $2 $3 $4
    touch $flag_filename
    echo "DbCreationUtility: Database was created"
else
    echo "DbCreationUtility: Database already exists"
fi