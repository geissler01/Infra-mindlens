import os
import re
import uuid

def generate_uuid(table_name, int_id):
    namespace = uuid.NAMESPACE_OID
    return str(uuid.uuid5(namespace, f"{table_name}_{int_id}"))

table_mapping = {
    'patients': '"Patients"',
    'treatments': '"Treatments"',
    'journalings': '"Journalings"',
    'questions': '"Questions"',
    'treatments_questions': '"TreatmentQuestions"',
    'journaling_answers': '"JournalingAnswers"',
    'journaling_register': '"JournalingRegisters"',
    'weekly_reports': '"WeeklyReports"',
    'weekly_cluster_reports': '"WeeklyClusterReports"',
    'pre_session_reports': '"PresessionReports"'
}

column_mapping = {
    'id': '"Id"',
    'global_user_id': '"GlobalUserId"',
    'phone': '"Phone"',
    'emergency_phone': '"EmergencyPhone"',
    'address': '"Address"',
    'age_range': '"AgeRange"',
    'relationship_status': '"RelationshipStatus"',
    'occupation_type': '"Occupation"',
    'living_situation': '"LivingSituation"',
    'primary_goal': '"PrimaryGoal"',
    'has_previous_therapy': '"HasPreviousTherapy"',
    'patient_id': '"PatientId"',
    'session_day': '"SessionDay"',
    'state': '"State"',
    'started_at': '"StartedAt"',
    'finished_at': '"FinishedAt"',
    'treatment_id': '"TreatmentId"',
    'date': '"Date"',
    'entry_type': '"EntryType"',
    'transcription': '"Transcription"',
    'voice_record_url': '"VoiceRecordKey"',
    'ai_reply_url': '"AiReplyKey"',
    'ai_reply_text': '"AiReplyText"',
    'status': '"State"',
    'question': '"Message"',
    'type': 'pillar_type',
    'question_id': '"QuestionId"',
    'journaling_id': '"JournalingId"',
    'pillar_type': 'pillar_type',
    'analyzed_content': '"AnalyzedContent"',
    'embedding': '"Embedding"',
    'summary': '"Summary"',
    'weekly_report_id': '"WeeklyReportId"',
    'title': '"Title"',
    'repetitions': '"Repetitions"',
    'cluster_embedding': '"ClusterEmbedding"',
    'flash_briefing': '"FlashBriefing"',
    'created_at': '"CreatedAt"',
    'updated_at': '"UpdatedAt"',
    'idempotency_key': '"IdempotencyKey"'
}

uuid_columns = {
    'patients': ['id', 'global_user_id'],
    'treatments': ['id', 'patient_id'],
    'journalings': ['id', 'treatment_id'],
    'questions': ['id'],
    'treatments_questions': ['id', 'treatment_id', 'question_id'],
    'journaling_answers': ['id', 'journaling_id', 'question_id'],
    'journaling_register': ['id', 'journaling_id'],
    'weekly_reports': ['id', 'treatment_id'],
    'weekly_cluster_reports': ['id', 'weekly_report_id'],
    'pre_session_reports': ['id', 'treatment_id']
}

# The implicit schema columns for INSERTs without columns (like from pg_dump)
implicit_columns = {
    'questions': ['id', 'question', 'type', 'created_at'],
    'journaling_answers': ['id', 'journaling_id', 'question_id', 'entry_type', 'idempotency_key', 'voice_record_url', 'transcription', 'status'],
    'journaling_register': ['id', 'journaling_id', 'pillar_type', 'analyzed_content', 'severity_score', 'confidence_score', 'embedding'],
    'weekly_cluster_reports': ['id', 'weekly_report_id', 'title', 'type', 'repetitions', 'cluster_embedding'],
    'treatments_questions': ['id', 'treatment_id', 'question_id'],
    'weekly_reports': ['id', 'treatment_id', 'summary', 'started_at', 'finished_at', 'created_at'],
    'pre_session_reports': ['id', 'treatment_id', 'flash_briefing', 'created_at']
}

columns_to_drop = ['severity_score', 'confidence_score']

def split_values(vals_str):
    vals = []
    current_val = []
    in_string = False
    escape = False
    
    for char in vals_str:
        if escape:
            current_val.append(char)
            escape = False
        elif char == "'":
            in_string = not in_string
            current_val.append(char)
        elif char == ',' and not in_string:
            vals.append(''.join(current_val).strip())
            current_val = []
        else:
            current_val.append(char)
    vals.append(''.join(current_val).strip())
    return vals

class SeedProcessor:
    def __init__(self):
        self.counters = {
            'patients': 1,
            'treatments': 1,
            'journalings': 1,
            'questions': 1,
            'journaling_answers': 1,
            'journaling_register': 1,
            'weekly_reports': 1,
            'weekly_cluster_reports': 1
        }

    def process_line(self, line):
        if not line.startswith('INSERT INTO'):
            return line
            
        # Match explicit columns: INSERT INTO table (cols) VALUES (vals);
        match_explicit = re.match(r'INSERT INTO\s+([a-zA-Z0-9_.]+)\s*\(([^)]+)\)\s*VALUES\s*\((.*)\);$', line.strip(), re.IGNORECASE | re.DOTALL)
        # Match implicit columns: INSERT INTO table VALUES (vals);
        match_implicit = re.match(r'INSERT INTO\s+([a-zA-Z0-9_.]+)\s*VALUES\s*\((.*)\);$', line.strip(), re.IGNORECASE | re.DOTALL)
        
        if match_explicit:
            table_name = match_explicit.group(1).lower().replace('public.', '')
            cols = [c.strip().lower() for c in match_explicit.group(2).split(',')]
            vals_str = match_explicit.group(3)
        elif match_implicit:
            table_name = match_implicit.group(1).lower().replace('public.', '')
            if table_name not in implicit_columns:
                return line
            cols = implicit_columns[table_name]
            vals_str = match_implicit.group(2)
        else:
            return line
            
        if table_name not in table_mapping:
            return line
            
        vals = split_values(vals_str)
        
        # Auto-inject ID if missing
        if 'id' not in cols:
            cols.insert(0, 'id')
            # Assign counter and increment
            new_id = self.counters.get(table_name, 1)
            vals.insert(0, str(new_id))
            self.counters[table_name] = new_id + 1
        else:
            # Update counter to max if explicit ID is used
            try:
                id_idx = cols.index('id')
                val = vals[id_idx].replace("'", "")
                if val.isdigit():
                    self.counters[table_name] = max(self.counters.get(table_name, 1), int(val) + 1)
            except:
                pass
                
        # Inject missing session_day
        if table_name == 'treatments' and 'session_day' not in cols:
            cols.append('session_day')
            vals.append("'monday'")
        
        new_cols = []
        new_vals = []
        
        for col, val in zip(cols, vals):
            if col in columns_to_drop:
                continue
                
            new_col = column_mapping.get(col, f'"{col}"')
            new_val = val
            
            if col in uuid_columns.get(table_name, []):
                if new_val != 'NULL':
                    new_val_int = new_val.replace("'", "")
                    
                    ref_table = table_name
                    if col.endswith('_id'):
                        ref_table = col[:-3] + 's'
                        if ref_table == 'patient_ids': ref_table = 'patients'
                        if ref_table == 'treatment_ids': ref_table = 'treatments'
                        if ref_table == 'question_ids': ref_table = 'questions'
                        if ref_table == 'journaling_ids': ref_table = 'journalings'
                        if ref_table == 'weekly_report_ids': ref_table = 'weekly_reports'
                    
                    new_uuid = generate_uuid(ref_table, new_val_int)
                    new_val = f"'{new_uuid}'"
            
            if col == 'status' or col == 'state':
                if new_val.strip("'").lower() == 'completed':
                    new_val = "'Processed'"
                if new_val.strip("'").lower() == 'active':
                    new_val = "'InProcess'"
                    
            if table_name == 'journaling_answers' and col == 'voice_record_url':
                new_col = '"VoiceRecordUrl"'
                
            # Date/Timestamp fixes
            if col == 'created_at':
                # Sometimes the timestamp comes as '2026-06-30 04:35:35.550895' which PostgreSQL might want differently or is fine.
                # Just leave it as is, pg usually parses it correctly.
                pass
                
            new_cols.append(new_col)
            new_vals.append(new_val)
            
        new_table_name = table_mapping[table_name]
        return f"INSERT INTO {new_table_name} ({', '.join(new_cols)}) VALUES ({', '.join(new_vals)});\n"

def process_file(input_path, output_path, processor):
    print(f"Processing {input_path}...")
    with open(input_path, 'r', encoding='utf-8') as f_in, open(output_path, 'w', encoding='utf-8') as f_out:
        buffer = ''
        for line in f_in:
            if buffer:
                buffer += line
                if buffer.strip().endswith(');'):
                    f_out.write(processor.process_line(buffer))
                    buffer = ''
            else:
                if line.strip().startswith('--'):
                    f_out.write(line)
                elif line.strip().startswith('SELECT setval') or line.strip().startswith('SELECT pg_catalog.setval'):
                    continue
                elif line.strip().startswith('SELECT pg_catalog.set_config'):
                    continue
                elif line.strip().startswith('INSERT INTO'):
                    if line.strip().endswith(');'):
                        f_out.write(processor.process_line(line))
                    else:
                        buffer = line
                else:
                    f_out.write(processor.process_line(line))

if __name__ == '__main__':
    base_dir = '/app/backend_poc_backup/database/seeds'
    out_dir = '/app/backend/database/seeds'
    
    if not os.path.exists(out_dir):
        os.makedirs(out_dir)
        
    # We must process files per tenant to maintain counters properly.
    tenants = ['a', 'b', 'c', 'd', 'e']
    
    for tenant in tenants:
        processor = SeedProcessor() # reset counters per tenant
        
        seed_file = f'0{2 + tenants.index(tenant)}-tenant_{tenant}_seed.sql'
        vector_file = f'07-vectors_tenant_{tenant}.sql'
        
        if os.path.exists(os.path.join(base_dir, seed_file)):
            process_file(os.path.join(base_dir, seed_file), os.path.join(out_dir, seed_file), processor)
            
        if os.path.exists(os.path.join(base_dir, vector_file)):
            process_file(os.path.join(base_dir, vector_file), os.path.join(out_dir, vector_file), processor)
            
    print("Conversion completed successfully!")
