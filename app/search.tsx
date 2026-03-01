import React, { useState, useMemo, useRef } from "react";
import {
  View,
  Text,
  StyleSheet,
  Pressable,
  useColorScheme,
  Platform,
  FlatList,
  TextInput,
} from "react-native";
import { useSafeAreaInsets } from "react-native-safe-area-context";
import { Ionicons, MaterialCommunityIcons } from "@expo/vector-icons";
import { router } from "expo-router";
import * as Haptics from "expo-haptics";
import Colors from "@/constants/colors";
import { useDocuments, Document } from "@/context/DocumentsContext";

const RECENT_SEARCHES = ["Invoice", "Contract", "Meeting Notes", "Tax Return"];

const FILTER_TYPES = [
  { label: "All", value: "" },
  { label: "PDF", value: "pdf" },
  { label: "Image", value: "image" },
  { label: "Word", value: "word" },
];

function DocTypeIcon({ type, size = 20 }: { type: string; size?: number }) {
  const color =
    type === "pdf" ? "#EF4444" : type === "image" ? "#8B5CF6" : type === "word" ? "#2563EB" : "#10B981";
  const icon =
    type === "pdf" ? "file-pdf-box" : type === "image" ? "file-image" : type === "word" ? "file-word" : "file-excel";
  return <MaterialCommunityIcons name={icon as any} size={size} color={color} />;
}

function SearchResultRow({ doc, query }: { doc: Document; query: string }) {
  const colorScheme = useColorScheme();
  const theme = colorScheme === "dark" ? Colors.dark : Colors.light;

  const highlightName = (name: string) => {
    if (!query) return name;
    const idx = name.toLowerCase().indexOf(query.toLowerCase());
    if (idx === -1) return name;
    return (
      <Text>
        <Text style={{ color: theme.text, fontFamily: "Inter_500Medium" }}>{name.slice(0, idx)}</Text>
        <Text style={{ color: theme.primary, fontFamily: "Inter_700Bold" }}>{name.slice(idx, idx + query.length)}</Text>
        <Text style={{ color: theme.text, fontFamily: "Inter_500Medium" }}>{name.slice(idx + query.length)}</Text>
      </Text>
    );
  };

  const formatDate = (ts: number) => {
    return new Date(ts).toLocaleDateString("en-US", { month: "short", day: "numeric" });
  };

  return (
    <Pressable
      style={[styles.resultRow, { backgroundColor: theme.card }]}
      onPress={() => router.push({ pathname: "/document/[id]", params: { id: doc.id } })}
    >
      <View style={[styles.resultIcon, { backgroundColor: theme.surfaceSecondary }]}>
        <DocTypeIcon type={doc.type} size={24} />
      </View>
      <View style={styles.resultInfo}>
        <Text style={[styles.resultName, { color: theme.text }]} numberOfLines={1}>
          {highlightName(doc.name)}
        </Text>
        <Text style={[styles.resultMeta, { color: theme.textMuted, fontFamily: "Inter_400Regular" }]}>
          {doc.pages} pages · {doc.size} · {formatDate(doc.updatedAt)}
        </Text>
      </View>
      {doc.starred && <Ionicons name="star" size={14} color="#F59E0B" style={{ marginRight: 4 }} />}
      <Ionicons name="chevron-forward" size={16} color={theme.textMuted} />
    </Pressable>
  );
}

export default function SearchScreen() {
  const colorScheme = useColorScheme();
  const isDark = colorScheme === "dark";
  const theme = isDark ? Colors.dark : Colors.light;
  const insets = useSafeAreaInsets();
  const { documents } = useDocuments();
  const [query, setQuery] = useState("");
  const [activeType, setActiveType] = useState("");
  const inputRef = useRef<TextInput>(null);

  const topPad = Platform.OS === "web" ? Math.max(insets.top, 67) : insets.top;

  const results = useMemo(() => {
    let docs = documents;
    if (activeType) docs = docs.filter((d) => d.type === activeType);
    if (query.trim()) {
      docs = docs.filter((d) => d.name.toLowerCase().includes(query.toLowerCase()));
    }
    return docs;
  }, [documents, query, activeType]);

  const hasQuery = query.trim().length > 0;

  return (
    <View style={[styles.container, { backgroundColor: theme.background }]}>
      {/* Search Header */}
      <View style={[styles.header, { paddingTop: topPad + 12, backgroundColor: theme.surface, borderBottomColor: theme.border }]}>
        <View style={[styles.searchBar, { backgroundColor: theme.surfaceSecondary }]}>
          <Ionicons name="search-outline" size={18} color={theme.textMuted} />
          <TextInput
            ref={inputRef}
            style={[styles.searchInput, { color: theme.text, fontFamily: "Inter_400Regular" }]}
            placeholder="Search documents..."
            placeholderTextColor={theme.textMuted}
            value={query}
            onChangeText={setQuery}
            autoFocus
            returnKeyType="search"
          />
          {query.length > 0 && (
            <Pressable onPress={() => setQuery("")}>
              <Ionicons name="close-circle" size={18} color={theme.textMuted} />
            </Pressable>
          )}
        </View>
        <Pressable style={styles.cancelBtn} onPress={() => router.back()}>
          <Text style={[styles.cancelText, { color: theme.primary, fontFamily: "Inter_500Medium" }]}>
            Cancel
          </Text>
        </Pressable>
      </View>

      {/* Type filters */}
      <View style={[styles.filters, { backgroundColor: theme.surface }]}>
        {FILTER_TYPES.map((f) => (
          <Pressable
            key={f.value}
            style={[
              styles.filterChip,
              { backgroundColor: activeType === f.value ? theme.primary : theme.surfaceSecondary }
            ]}
            onPress={() => { setActiveType(f.value); Haptics.impactAsync(Haptics.ImpactFeedbackStyle.Light); }}
          >
            <Text style={[
              styles.filterChipText,
              { color: activeType === f.value ? "#fff" : theme.textSecondary, fontFamily: "Inter_500Medium" }
            ]}>
              {f.label}
            </Text>
          </Pressable>
        ))}
      </View>

      {!hasQuery && !activeType ? (
        /* Pre-search state */
        <View style={styles.preSearch}>
          <Text style={[styles.preSearchTitle, { color: theme.text, fontFamily: "Inter_600SemiBold" }]}>
            Recent Searches
          </Text>
          {RECENT_SEARCHES.map((s) => (
            <Pressable
              key={s}
              style={[styles.recentSearchRow, { borderBottomColor: theme.border }]}
              onPress={() => setQuery(s)}
            >
              <Ionicons name="time-outline" size={16} color={theme.textMuted} />
              <Text style={[styles.recentSearchText, { color: theme.text, fontFamily: "Inter_400Regular" }]}>
                {s}
              </Text>
              <Ionicons name="arrow-up-back" size={14} color={theme.textMuted} />
            </Pressable>
          ))}

          <Text style={[styles.preSearchTitle, { color: theme.text, fontFamily: "Inter_600SemiBold", marginTop: 28 }]}>
            All Documents
          </Text>
          <FlatList
            data={documents.slice(0, 5)}
            keyExtractor={(d) => d.id}
            scrollEnabled={false}
            contentContainerStyle={{ gap: 8, marginTop: 4 }}
            renderItem={({ item }) => <SearchResultRow doc={item} query="" />}
          />
        </View>
      ) : (
        /* Search results */
        <FlatList
          data={results}
          keyExtractor={(d) => d.id}
          contentContainerStyle={{
            paddingHorizontal: 16,
            paddingTop: 12,
            paddingBottom: Platform.OS === "web" ? 34 : insets.bottom + 20,
            gap: 8,
          }}
          showsVerticalScrollIndicator={false}
          ListHeaderComponent={
            hasQuery ? (
              <Text style={[styles.resultCount, { color: theme.textMuted, fontFamily: "Inter_400Regular" }]}>
                {results.length} result{results.length !== 1 ? "s" : ""} for "{query}"
              </Text>
            ) : null
          }
          renderItem={({ item }) => <SearchResultRow doc={item} query={query} />}
          ListEmptyComponent={
            <View style={styles.emptyState}>
              <Ionicons name="search-outline" size={52} color={theme.textMuted} />
              <Text style={[styles.emptyTitle, { color: theme.text, fontFamily: "Inter_600SemiBold" }]}>
                No results
              </Text>
              <Text style={[styles.emptySubtitle, { color: theme.textMuted, fontFamily: "Inter_400Regular" }]}>
                Try a different name or remove filters
              </Text>
            </View>
          }
        />
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  header: {
    flexDirection: "row",
    alignItems: "center",
    paddingHorizontal: 16,
    paddingBottom: 12,
    gap: 10,
    borderBottomWidth: StyleSheet.hairlineWidth,
  },
  searchBar: {
    flex: 1,
    flexDirection: "row",
    alignItems: "center",
    borderRadius: 13,
    paddingHorizontal: 12,
    paddingVertical: 10,
    gap: 8,
  },
  searchInput: { flex: 1, fontSize: 15 },
  cancelBtn: { paddingLeft: 4 },
  cancelText: { fontSize: 15 },
  filters: {
    flexDirection: "row",
    paddingHorizontal: 16,
    paddingVertical: 10,
    gap: 8,
  },
  filterChip: { paddingHorizontal: 14, paddingVertical: 6, borderRadius: 20 },
  filterChipText: { fontSize: 13 },
  preSearch: { paddingHorizontal: 16, paddingTop: 20 },
  preSearchTitle: { fontSize: 16, marginBottom: 12 },
  recentSearchRow: {
    flexDirection: "row",
    alignItems: "center",
    gap: 12,
    paddingVertical: 12,
    borderBottomWidth: StyleSheet.hairlineWidth,
  },
  recentSearchText: { flex: 1, fontSize: 15 },
  resultCount: { fontSize: 13, marginBottom: 8 },
  resultRow: { flexDirection: "row", alignItems: "center", borderRadius: 14, padding: 14, gap: 12 },
  resultIcon: { width: 46, height: 46, borderRadius: 12, alignItems: "center", justifyContent: "center" },
  resultInfo: { flex: 1 },
  resultName: { fontSize: 14, marginBottom: 4 },
  resultMeta: { fontSize: 12 },
  emptyState: { alignItems: "center", paddingTop: 60, gap: 10 },
  emptyTitle: { fontSize: 18 },
  emptySubtitle: { fontSize: 14, textAlign: "center", paddingHorizontal: 40 },
});
