import psycopg2
conn = psycopg2.connect(host="postgres-db", port="5432", dbname="db_mindlens_carlos", user="journal_user", password="journal_pass")
cur = conn.cursor()
cur.execute("SELECT column_name FROM information_schema.columns WHERE table_name = 'Journalings';")
print(cur.fetchall())
