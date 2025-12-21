FROM postgres:17
RUN apt-get update -y && apt-get upgrade -y && apt-get install -y postgresql-$PG_MAJOR-pgvector && apt-get clean && rm -rf /var/lib/apt/lists/*
