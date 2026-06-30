import psycopg2
conn = psycopg2.connect(host="postgres-db", port="5432", dbname="master_db", user="journal_user", password="journal_pass")
cur = conn.cursor()
cur.execute('SELECT "Id", "Email", "Role" FROM "AspNetUsers";')
rows = cur.fetchall()
for r in rows:
    print(r)
cur.close()
conn.close()
