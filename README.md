# Ferrous
Ferrous is just a Hospitality manager!

[![CircleCI](https://circleci.com/gh/pulsejet/ferrous.svg?style=shield&circle-token=f96a250457ddb62753a7df5c7c65415a21c2545d)](https://circleci.com/gh/pulsejet/ferrous)
[![Build Status](https://travis-ci.com/pulsejet/ferrous.svg?token=BsU3eVxjdePqSu3v4M8V&branch=master)](https://travis-ci.com/pulsejet/ferrous)

## Introduction
One of the biggest hurdles of managing large events which provide on-site accomodation is to properly distribute the available space with optimization to everyone. Ferrous aims to solve this problem by serving as a single platform to take control of everything from registering for accomodations to helping distribute them to guests.

## Purpose
Ferrous was originally created as a proof of concept for the Hospitality department of Mood Indigo, the student-organized cultural fest of Indian Institute of Technology, Bombay. The main purpose it serves is to easily provide accomodation to everyone with minimal delay within the highly limited resources avaiable to the organizers.

## Structure
The server written in ASP.NET Core works as a RESTful API, and is theorotically client independent, though it works closely with [ferrous-client](https://github.com/pulsejet/ferrous-client), which is an Angular 5 PWA. The server provides hyperlinks (HATEOAS) for almost everything and this is the recommended way of routing. Following are the main entities:

#### Contingent
A group of people who are supposed to arrive together. Each contingent has a unique Contingent Leader, with a Contingent Leader number (commonly CLNo), who is supposed to co-ordinate with the organizers.
#### Person
An entity attending the event. Has a unique Mood Indigo number (MINo).
#### Room
A unique room, which may be allocated to multiple people.

## Features
1. Location maps for easy allocation.
2. Rooms may be allocated to multiple contingents partially, in case this is necessary.
3. Live maps for survey by organizers.
4. Permissions and elevation levels, for a heirarcy in organizers.
5. Chunked arrivals, where contingents may arrive in chunks of few people.
6. Export data to spreadsheet

## Usage
The project can be built directly with the dotnet sdk used by `Ferrous.csproj`. It is recommended to use the client supplied, since development of the server takes place in close co-ordination with it. During development the client needs to be built and served separately, and traffic not relevant for the server is passed to `localhost:4200`, where the client should be served. For production, `dotnet publish` will automatically run the build script for `ClientApp` and the client will be bundled with the application, which can then be deployed directly, preferably behind an SSL reverse proxy.

## Contributing
Pull requests and issues are welcome, as long as a few constraints are followed:
* If you are breaking a feature used by the provided client, be sure to state this explicitly. Your pull request will be merged only when the client can be modified accordingly.
* Follow the general style of the project. Badly written or undocumented code might be rejected.
* If you are proposing a new model or modifications to an existing one, create an issue first explaining why it is useful.
* Your code should not break CI builds (there can be exceptions).
* Outdated, unsupported or closed-source libraries should not be used.
* Be nice!
