#!/bin/bash
if test "$OS" = "Windows_NT"
then
  # use .Net

  .paket/paket.exe restore group Build
  exit_code=$?
  if [ $exit_code -ne 0 ]; then
  	exit $exit_code
  fi

  packages/build/FAKE/tools/FAKE.exe $@ --fsiargs build.fsx
else

  mono .paket/paket.exe restore group Build
  exit_code=$?
  if [ $exit_code -ne 0 ]; then
  	exit $exit_code
  fi
  mono packages/build/FAKE/tools/FAKE.exe $@ --fsiargs -d:MONO build.fsx
fi
