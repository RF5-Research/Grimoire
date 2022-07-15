@echo off
SET cwd=%~dp0
if [%1]==[] goto :eof
:loop
%cwd%enum2json.py %1
shift
if not [%1]==[] goto loop